## Юз-кейс 2: Комбинация эффектов (Финальная архитектура)

### Цель

Продемонстрировать, как финальная архитектура обрабатывает одновременное применение нескольких эффектов (постоянного и одноразового) к одному символу, и как данные от разных эффектов корректно доставляются и применяются через свои аппликаторы.

### Строка для анимации

`{charbychar speed=0.1 anim=fadein}I'm typing while i'm {wave}vibing...{/wave}{/charbychar}`

### 1. Шаги реализации (Пользователь)

#### 1.1. Создание Наборов Свойств и Модификаторов (Код)

Помимо `TransformPropertySet` и `WaveModifier` из юз-кейса 1, пользователь создает новые классы для эффекта появления.

```csharp
// 1. Новый контракт данных для цвета
public class ColorPropertySet : IPropertySet
{
    public Color Color = Color.white;
}

// 2. Новый модификатор для анимации появления
public class FadeInModifier : IAppearanceModifier // Наследуется от ICharacterModifier
{
    public void Modify(CharacterPropertyBag propertyBag, float normalizedTime)
    {
        var colorSet = propertyBag.GetOrCreatePropertySet<ColorPropertySet>();
        // Изменяем только альфа-канал
        colorSet.Color.a = Mathf.Lerp(0, 1, normalizedTime);
    }
}
```

#### 1.2. Создание Аппликаторов (Код)

Пользователь должен убедиться, что у него есть аппликаторы для всех типов данных, которые он использует.

```csharp
// Аппликатор для цвета (новый)
public class ColorToTMProApplicator : IPropertyApplicator
{
    public Type PropertySetType => typeof(ColorPropertySet);

    public void Apply(ITextOutput target, IPropertySet propertySet, int charIndex)
    {
        if (!(target is TextMeshProOutput tmproOutput) || !(propertySet is ColorPropertySet colorSet)) return;
        
        // Логика изменения цвета вершин для TMPro
        // ...
    }
}

// Аппликатор для трансформаций (из юз-кейса 1)
public class TransformToTMProApplicator : IPropertyApplicator { /* ... */ }
```

#### 1.3. Композиция в `TextAnimator` (Код)

Ключевой момент: в `TextAnimator` пользователь регистрирует **все** необходимые для его эффектов аппликаторы.

```csharp
// Файл: TextAnimator.cs
void Start()
{
    // ...
    var applicators = new List<IPropertyApplicator>
    {
        new TransformToTMProApplicator(), // Для тега {wave}
        new ColorToTMProApplicator()      // Для анимации появления fadein
    };

    ITextOutput textOutput = new TextMeshProOutput(_textComponent, applicators);
    // ...
}
```

### 2. Как это работает внутри системы

Рассмотрим кадр, в котором символ `'v'` из слова `vibing...` только что появился и находится в середине своей анимации появления.

1.  **Сборка данных (для символа `'v'`):**
    1.  `PlaybackEngine` создает пустой `CharacterPropertyBag`.
    2.  **Применяется `WaveModifier`:** Движок находит тег `{wave}`. Модификатор волны вызывается и добавляет/изменяет `TransformPropertySet` в `bag`.
    3.  **Применяется `FadeInModifier`:** Движок видит, что для `'v'` активна анимация появления. Модификатор `FadeInModifier` вызывается и добавляет/изменяет `ColorPropertySet` в том же самом `bag`.
    4.  В итоге `bag` содержит **два** разных объекта: `TransformPropertySet` с данными о смещении и `ColorPropertySet` с данными о цвете/прозрачности.

2.  **Диспетчеризация (для символа `'v'`):**
    1.  Движок вызывает `textOutput.Dispatch(transformSet, charIndex)`.
    2.  `TextMeshProOutput` находит в своем словаре `TransformToTMProApplicator` и вызывает его. Аппликатор изменяет **позиции вершин**.
    3.  Движок вызывает `textOutput.Dispatch(colorSet, charIndex)`.
    4.  `TextMeshProOutput` находит `ColorToTMProApplicator` и вызывает его. Аппликатор изменяет **цвет вершин**.

3.  **Финализация:** В конце кадра `textOutput.FinalizeUpdate()` применяет все накопленные изменения вершин (и позиций, и цвета) на GPU.

### Заключение

Этот сценарий доказывает, что архитектура позволяет комбинировать произвольное количество эффектов. Каждый эффект работает с собственным, изолированным набором данных, а их применение к конечному тексту полностью разделено и управляется через независимые классы-аппликаторы, что является вершиной OCP-совместимости.