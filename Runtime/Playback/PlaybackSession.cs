using System;
using System.Collections.Generic;
using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Output;
using FLFloppa.TextAnimator.Playback.Systems;

namespace FLFloppa.TextAnimator.Playback
{
    /// <summary>
    /// Represents an active playback session that can be advanced over time.
    /// </summary>
    public sealed class PlaybackSession : IPlaybackSessionControl
    {
        private readonly ITextOutput _textOutput;
        private readonly IReadOnlyList<IPlaybackInstruction> _instructions;
        private readonly IReadOnlyList<CharacterDescriptor> _characters;
        private readonly IReadOnlyDictionary<TagNode, ISubsystemModifier> _modifiers;
        private readonly CharacterRevealClock _revealClock;
        private readonly ICharacterAnimationPipeline _animationPipeline;

        private int _currentInstructionIndex;
        private float _instructionElapsed;
        private float _currentInstructionStartTime;
        private float _elapsedTime;
        private int _visibleCharacterCount;

        internal PlaybackSession(
            ITextOutput textOutput,
            ICharacterAnimationPipeline animationPipeline,
            IReadOnlyDictionary<TagNode, ISubsystemModifier> modifiers,
            IReadOnlyList<IPlaybackInstruction> instructions,
            IReadOnlyList<CharacterDescriptor> characters)
        {
            if (textOutput == null)
            {
                throw new ArgumentNullException(nameof(textOutput));
            }

            if (animationPipeline == null)
            {
                throw new ArgumentNullException(nameof(animationPipeline));
            }

            if (modifiers == null)
            {
                throw new ArgumentNullException(nameof(modifiers));
            }

            if (instructions == null)
            {
                throw new ArgumentNullException(nameof(instructions));
            }

            if (characters == null)
            {
                throw new ArgumentNullException(nameof(characters));
            }

            _textOutput = textOutput;
            _instructions = instructions;
            _characters = characters;
            _modifiers = modifiers;
            _revealClock = new CharacterRevealClock(_characters.Count);
            _animationPipeline = animationPipeline;
            _currentInstructionStartTime = float.NaN;
        }

        /// <summary>
        /// Indicates whether all timeline instructions have been processed.
        /// </summary>
        private bool InstructionsComplete => _currentInstructionIndex >= _instructions.Count;
        private bool ShouldHoldSession => _modifiers.Count > 0 && _visibleCharacterCount > 0;

        public bool IsComplete => InstructionsComplete && !ShouldHoldSession;

        /// <summary>
        /// Total elapsed time since the session started.
        /// </summary>
        public float ElapsedTime => _elapsedTime;

        /// <summary>
        /// Advances the playback session by the provided delta time.
        /// </summary>
        /// <returns>True when the session has completed playback.</returns>
        public bool Update(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            if (IsComplete)
            {
                _textOutput.BeginFrame();
                ApplyCharacterModifiers();
                _textOutput.FinalizeUpdate();
                return true;
            }

            var instructionsBeforeUpdate = _currentInstructionIndex;
            _elapsedTime += deltaTime;
            ProcessInstructions(deltaTime);
            
            if (_currentInstructionIndex != instructionsBeforeUpdate)
            {
            }
            
            _textOutput.BeginFrame();
            ApplyCharacterModifiers();
            _textOutput.FinalizeUpdate();
            
            return IsComplete;
        }

        public void RevealCharacter(int characterIndex, Action<string> onCharacterRevealed = null)
        {
            if (characterIndex < 0 || characterIndex >= _characters.Count)
            {
                return;
            }

            var desiredVisible = characterIndex + 1;
            if (desiredVisible > _visibleCharacterCount)
            {
                _visibleCharacterCount = desiredVisible;
                _textOutput.SetVisibleCharacterCount(_visibleCharacterCount);
                
            }

            // Record reveal time only once, and use elapsed time BEFORE current instruction started
            if (!_revealClock.HasRevealTime(characterIndex))
            {
                var revealStartTime = float.IsNaN(_currentInstructionStartTime)
                    ? _elapsedTime - _instructionElapsed
                    : _currentInstructionStartTime;
                _revealClock.SetRevealTime(characterIndex, revealStartTime);
            }

            if (onCharacterRevealed != null)
            {
                var descriptor = _characters[characterIndex];
                onCharacterRevealed(descriptor.Character.ToString());
            }
        }

        private void ProcessInstructions(float deltaTime)
        {
            var remaining = deltaTime;
            while (remaining > 0f && _currentInstructionIndex < _instructions.Count)
            {
                var instruction = _instructions[_currentInstructionIndex];
                
                if (_instructionElapsed == 0f)
                {
                    _currentInstructionStartTime = _elapsedTime - remaining;
                }
                
                var timeNeeded = instruction.Duration - _instructionElapsed;
                if (remaining >= timeNeeded)
                {
                    remaining -= timeNeeded;
                    _instructionElapsed += timeNeeded;
                    instruction.Execute(this);
                    _currentInstructionIndex++;
                    _instructionElapsed = 0f;
                    _currentInstructionStartTime = float.NaN;
                    
                    // Execute all immediately following zero-duration instructions
                    while (_currentInstructionIndex < _instructions.Count)
                    {
                        var nextInstruction = _instructions[_currentInstructionIndex];
                        if (nextInstruction.Duration > 0f)
                        {
                            break;
                        }
                        
                        nextInstruction.Execute(this);
                        _currentInstructionIndex++;
                    }
                }
                else
                {
                    _instructionElapsed += remaining;
                    remaining = 0f;
                }
            }
        }

        private void ApplyCharacterModifiers()
        {
            if (_visibleCharacterCount <= 0)
            {
                return;
            }

            var upperBound = Math.Min(_visibleCharacterCount, _characters.Count);
            
            for (var characterIndex = 0; characterIndex < upperBound; characterIndex++)
            {
                var descriptor = _characters[characterIndex];
                _animationPipeline.Apply(characterIndex, descriptor, _elapsedTime, _revealClock, _modifiers, _textOutput);
            }
        }
    }
}