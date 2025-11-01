using System;

namespace FLFloppa.TextAnimator.Animator.Subsystems
{
    /// <summary>
    /// Represents a stable identifier for a text animator subsystem type.
    /// </summary>
    public readonly struct TextAnimatorSubsystemKey : IEquatable<TextAnimatorSubsystemKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextAnimatorSubsystemKey"/> struct.
        /// </summary>
        /// <param name="value">The identifier string.</param>
        public TextAnimatorSubsystemKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Subsystem key cannot be null or whitespace.", nameof(value));
            }

            Value = value;
        }

        /// <summary>
        /// Gets the string value representing the subsystem key.
        /// </summary>
        public string Value { get; }

        /// <inheritdoc />
        public bool Equals(TextAnimatorSubsystemKey other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is TextAnimatorSubsystemKey other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Determines whether the provided key is considered valid.
        /// </summary>
        /// <param name="key">The key to validate.</param>
        /// <returns><c>true</c> when the key contains a non-empty value; otherwise, <c>false</c>.</returns>
        public static bool IsValid(TextAnimatorSubsystemKey key)
        {
            return !string.IsNullOrWhiteSpace(key.Value);
        }
    }

    /// <summary>
    /// Provides built-in subsystem keys used by the text animator runtime.
    /// </summary>
    public static class TextAnimatorSubsystemKeys
    {
        /// <summary>
        /// Identifies subsystems that manipulate per-character transforms.
        /// </summary>
        public static readonly TextAnimatorSubsystemKey Transform = new TextAnimatorSubsystemKey("Transform");

        /// <summary>
        /// Identifies subsystems that control per-character color output.
        /// </summary>
        public static readonly TextAnimatorSubsystemKey Color = new TextAnimatorSubsystemKey("Color");

        /// <summary>
        /// Identifies subsystems that control material-related overrides.
        /// </summary>
        public static readonly TextAnimatorSubsystemKey Material = new TextAnimatorSubsystemKey("Material");

        /// <summary>
        /// Identifies the legacy monolithic subsystem preserved for backward compatibility.
        /// </summary>
        public static readonly TextAnimatorSubsystemKey Legacy = new TextAnimatorSubsystemKey("Legacy");
    }
}
