using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcane_Coop.Models;

public class RuneProtocolLevel
{
    public int LevelNumber { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public LogicRule[] AlphaRules { get; set; } = Array.Empty<LogicRule>(); // Player A (Piltover) rules
    public LogicRule[] BetaRules { get; set; } = Array.Empty<LogicRule>();  // Player B (Zaunite) rules
    public bool[] Solution { get; set; } = new bool[8]; // The unique correct solution
    public string SolutionExplanation { get; set; } = "";
}

public class LogicRule
{
    public string Id { get; set; } = "";
    public RuleType Type { get; set; }
    public string Description { get; set; } = "";
    public int[] InvolvedRunes { get; set; } = Array.Empty<int>(); // Which runes this rule references
    public bool IsValidated { get; set; } = false; // Whether this rule is currently satisfied
    public string ValidationMessage { get; set; } = ""; // Feedback for the rule
    
    // Rule-specific data
    public int? RequiredCount { get; set; } // For COUNT_EXACT rules
    public int[] RequiredStates { get; set; } = Array.Empty<int>(); // For complex conditional rules
    public bool? RequiredState { get; set; } // For simple state requirements
    
    public LogicRule(string id, RuleType type, string description, int[] involvedRunes)
    {
        Id = id;
        Type = type;
        Description = description;
        InvolvedRunes = involvedRunes;
    }
}

public enum RuleType
{
    COUNT_EXACT,    // Exactly N runes must be UP
    CONDITIONAL_IF, // IF condition THEN consequence
    EITHER_OR,      // Either A or B (but not both)
    SAME_STATE,     // Multiple runes must be in same state
    DIFFERENT_STATE // Multiple runes must be in different states
}

public class RuleValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = "";
    public int[] AffectedRunes { get; set; } = Array.Empty<int>();
}

public class RuneProtocolPlayerView
{
    public string Role { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Instruction { get; set; } = "";
    public LogicRule[] Rules { get; set; } = Array.Empty<LogicRule>();
    public int[] ControllableRunes { get; set; } = Array.Empty<int>();
    public bool[] RuneStates { get; set; } = new bool[8];
    public int SatisfiedRules { get; set; }
    public int TotalRules { get; set; }
    public bool IsCompleted { get; set; }
    public int Score { get; set; }
    public int CurrentLevel { get; set; }
    public int MaxLevel { get; set; }
    public string[] RuleValidationMessages { get; set; } = Array.Empty<string>();
    public bool ShowHint { get; set; } = false;
    public string HintMessage { get; set; } = "";
}

public class RuneProtocolGameState
{
    public bool[] RuneStates { get; set; } = new bool[8];
    public int CurrentLevel { get; set; }
    public int MaxLevel { get; set; }
    public bool IsCompleted { get; set; }
    public int Score { get; set; }
    public int PlayerCount { get; set; }
    public int PlayersNeeded { get; set; }
    public int SatisfiedRules { get; set; }
    public int TotalRules { get; set; }
    public string LevelTitle { get; set; } = "";
    public string LevelDescription { get; set; } = "";
    public bool AllRulesSatisfied { get; set; } = false;
    public string CompletionMessage { get; set; } = "";
}

public class LogicPuzzleSolver
{
    public static RuleValidationResult ValidateRule(LogicRule rule, bool[] runeStates)
    {
        var result = new RuleValidationResult();
        
        switch (rule.Type)
        {
            case RuleType.COUNT_EXACT:
                var upCount = rule.InvolvedRunes.Count(r => runeStates[r]);
                result.IsValid = upCount == rule.RequiredCount;
                result.Message = result.IsValid 
                    ? $"✓ Exactly {rule.RequiredCount} runes are UP"
                    : $"✗ Need exactly {rule.RequiredCount} UP, have {upCount}";
                result.AffectedRunes = rule.InvolvedRunes;
                break;
                
            case RuleType.CONDITIONAL_IF:
                // Format: IF rune[0] is state[0] THEN rune[1] must be state[1]
                if (rule.InvolvedRunes.Length >= 2 && rule.RequiredStates.Length >= 2)
                {
                    var conditionRune = rule.InvolvedRunes[0];
                    var consequenceRune = rule.InvolvedRunes[1];
                    var conditionState = rule.RequiredStates[0] == 1; // 1 = UP, 0 = DOWN
                    var requiredConsequenceState = rule.RequiredStates[1] == 1;
                    
                    if (runeStates[conditionRune] == conditionState)
                    {
                        // Condition is met, check consequence
                        result.IsValid = runeStates[consequenceRune] == requiredConsequenceState;
                        result.Message = result.IsValid
                            ? $"✓ Condition met and consequence satisfied"
                            : $"✗ R{conditionRune + 1} is {(conditionState ? "UP" : "DOWN")}, so R{consequenceRune + 1} must be {(requiredConsequenceState ? "UP" : "DOWN")}";
                    }
                    else
                    {
                        // Condition not met, rule is satisfied
                        result.IsValid = true;
                        result.Message = $"✓ Condition not triggered";
                    }
                    result.AffectedRunes = rule.InvolvedRunes;
                }
                break;
                
            case RuleType.EITHER_OR:
                var upRunes = rule.InvolvedRunes.Where(r => runeStates[r]).ToArray();
                result.IsValid = upRunes.Length == 1; // Exactly one must be UP
                result.Message = result.IsValid
                    ? $"✓ Exactly one rune is UP as required"
                    : upRunes.Length == 0 
                        ? $"✗ One of these runes must be UP"
                        : $"✗ Only one rune can be UP, {upRunes.Length} are UP";
                result.AffectedRunes = rule.InvolvedRunes;
                break;
                
            case RuleType.SAME_STATE:
                var firstState = runeStates[rule.InvolvedRunes[0]];
                result.IsValid = rule.InvolvedRunes.All(r => runeStates[r] == firstState);
                result.Message = result.IsValid
                    ? $"✓ All runes are in the same state"
                    : $"✗ All specified runes must be in the same state";
                result.AffectedRunes = rule.InvolvedRunes;
                break;
        }
        
        return result;
    }
    
    public static bool ValidateCompleteSolution(RuneProtocolLevel level, bool[] runeStates)
    {
        // Validate all rules - this is the primary method now
        var allRules = level.AlphaRules.Concat(level.BetaRules);
        return allRules.All(rule => ValidateRule(rule, runeStates).IsValid);
    }
}