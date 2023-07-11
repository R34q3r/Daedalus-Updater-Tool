using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Reflection.Metadata.Ecma335;

namespace DaedalusUpdateTool
{
    public class HeaderFile
    {
        public string Name { get; set; }
        public List<string> Lines { get; set; } = new List<string>();
    }
    public class ReplacedString
    {
        public string Original { get; set; }
        public string New { get; set; }
    }
    public class Function
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<Variable> Arguments { get; set; } = new List<Variable>();
        public string FunctionPath { get; set; }
    }
    public class BannedClass
    {
        public string Name { get; set; }
        public string Size { get; set; }
    }
    public class Variable
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
    public class ClassFile
    {
        public string FileName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string ParentClass { get; set; } = string.Empty;
        public List<string> ChildClasses { get; set; } = new List<string>();
        public List<BannedClass> BannedClasses { get; set; }  = new List<BannedClass>();
        public List<string> Lines { get; set; } = new List<string>();
        public List<Function>  Functions { get; set; } = new List<Function>();
        public bool IsReal { get; set; } = true;
        public bool IsEnum { get; set; } = false;
    }

    public static class ModdingTools
    {
        public static List<ReplacedString> ReplacedStrings = new List<ReplacedString> {
            new ReplacedString {Original = "%", New = "Percent" },
            new ReplacedString {Original = "-", New = "Dash" },
            new ReplacedString {Original = "?", New = "QuestionMark" },
            new ReplacedString {Original = ")", New = "RightParenthesis" },
            new ReplacedString {Original = "(", New = "LeftParenthesis" },
            new ReplacedString {Original = "100", New = "OneHundred" },
            new ReplacedString {Original = "10", New = "Ten" },
            new ReplacedString {Original = "20", New = "Twenty" },
            new ReplacedString {Original = "30", New = "Thirty" },
            new ReplacedString {Original = "40", New = "Forty" },
            new ReplacedString {Original = "60", New = "Sixty" },
            new ReplacedString {Original = "80", New = "Eighty" },
            new ReplacedString {Original = "3dTiles", New = "ThreeDTiles" },
            new ReplacedString {Original = "0;", New = "Zero;" },
            new ReplacedString {Original = "FALSE;", New = "FALSO;" },
            new ReplacedString {Original = "TRUE;", New = "VERDADERO;" },

        };
        public static List<string> ReplacedClasses = new List<string> {
            "UObject",
            "AActor",
            "UGameInstance",
            "AGameModeBase",
            "UGameplayStatics",
            "APlayerController",
            "AGameMode",
            "APawn",
            "AGameState",
            "UWorld",
            "FGuid",
            "FVector",
            "FTransform",
            "EMouseCursor",
            "FRotator",
            "FFrameNumber",
            "FFrameTime",
            "FLinearColor",
            "FQuat",
            "FVector2D",
            "UFunction",
            "FRandomStream",
            "FBoxSphereBounds",
            "EAutomationEventType",
            "EAxis",
            "EInterpCurveMode",
            "ELifetimeCondition",
            "ELocalizedTextSourceCategory",
            "ELogTimes",
            "ERangeBoundTypes",
            "EUnit",
            "EPixelFormat",
            "ESearchDir",
            "ESearchCase",
            "ESpawnActorCollisionHandlingMethod",
            "FJoinabilitySettings",
            "FUniqueNetIdWrapper",
            "FVector4",
            "FTwoVectors",
            "FPlane",
            "FPackedNormal",
            "FPackedRGB10A2N",
            "FPackedRGBA16N",
            "FIntPoint",
            "FIntVector",
            "FColor",
            "FBox",
            "FBox2D",
            "FOrientedBox",
            "FMatrix",
            "FInterpCurvePointFloat",
            "FInterpCurveFloat",
            "FInterpCurvePointVector2D",
            "FInterpCurveVector2D",
            "FInterpCurvePointVector",
            "FInterpCurveVector",
            "FInterpCurvePointQuat",
            "FInterpCurveQuat",
            "FInterpCurvePointTwoVectors",
            "FInterpCurveTwoVectors",
            "FInterpCurvePointLinearColor",
            "FInterpCurveLinearColor",
            "FDateTime",
            "FFrameRate",
            "FQualifiedFrameTime",
            "FTimecode",
            "FTimespan",
            "FSoftClassPath",
            "FPrimaryAssetType",
            "FPrimaryAssetId",
            "FFallbackStruct",
            "FFloatRangeBound",
            "FFloatRange",
            "FInt32RangeBound",
            "FInt32Range",
            "FFloatInterval",
            "FInt32Interval",
            "FPolyglotTextData",
            "FAutomationEvent",
            "FAutomationExecutionEntry",
            "UClass",
            "UField",
            "UStruct",
            "FField",
            "ULevel",
            "UWorld",
            "ACustomClass",
            "UBlueprintFunctionLibrary",
            "EAutomationEventType",
            "FKey",
            "",
            "",
            "",
            "",
            "",
            "",

        };
        public static List<string> BannedClasses = new List<string> {
            "TWeakObjectPtr",
            "FMulticastInlineDelegate",
            "FDelegate",
            "FSoftObjectPath",
            "TSoftClassPtr",
            "TSoftObjectPtr",
            "LazyObjectProperty",
            "TScriptInterface",
            "TFieldPath",
            "Function BP_PlayerBuildingPlacement.BP_PlayerBuildingPlacement_C.debug spawn rows of frames, then walls",
            "UberGraphFrame",
            "TSet",
            "3dTiles",
            "Function BP_LightningStrike.BP_LightningStrike_C.TestStrike(works once)",
            "FPointerToUberGraphFrame",
            "FMulticastSparseDelegate"
        };
        public static string[] RemoveEmptyString(string[] strings)
        {
            List<string> returnStrings = new List<string>();

            foreach(string x in strings)
            {
                if(x != "")
                {
                    returnStrings.Add(x);
                }
            }

            return returnStrings.ToArray();
        }
        public static string ReplaceStrings (string original)
        {
            foreach(ReplacedString replacedString in ReplacedStrings)
            {
                original = original.Replace(replacedString.Original, replacedString.New);
            }

            return original;
        }
        public static bool IsReplaced(string line)
        {
            foreach (string className in ReplacedClasses)
            {
                if (line.Contains("struct "+ className +" {") || line.Contains("struct " + className + " : ") || line.Contains("enum class " + className +" : uint8 {") || line.Contains("enum class " + className + " : int32"))
                {
                    return true;
                }
            }

            return false;
        }
        public static bool IsBanned(string line)
        {
            foreach(string className in BannedClasses)
            {
                if(line.Contains(className))
                {
                    return true;
                }
            }

            return false;
        }
        public static string GetTypeWithoutPre(string variable)
        {
            if(variable.Contains("struct "))
            {
                string variableWithoutPre = variable.Substring(8);

                return variableWithoutPre;
            }
            else if(variable.Contains("enum class "))
            {
                string variableWithoutPre = variable.Substring(12);

                return variableWithoutPre;
            }

            return variable;
        }
        public static Variable GetVariableValues(string argumentString)
        {
            Variable argument = new Variable();

            string[] splitArgument = ModdingTools.RemoveEmptyString(argumentString.Split(" "));

            // Parameter is a TArray or TSet //
            if (argumentString.Contains("TArray") || argumentString.Contains("TSet"))
            {
                // TArray / TSet format ( enum ) //
                if (argumentString.Contains("<enum"))
                {
                    if (splitArgument.Length > 5)
                        argument.Name = string.Join("_", splitArgument, 4, splitArgument.Length - 4);
                    else
                        argument.Name = splitArgument[4];

                    argument.Type = string.Join(" ", splitArgument, 0, 4);
                }
                // TArray / TSet format ( struct ) //
                else if (argumentString.Contains("<struct"))
                {
                    if (splitArgument.Length > 4)
                        argument.Name = string.Join("_", splitArgument, 3, splitArgument.Length - 3);
                    else
                        argument.Name = splitArgument[3];

                    argument.Type = string.Join(" ", splitArgument, 0, 3);
                }
                // TArray / TSet format ( generic ) //
                else
                {
                    if (splitArgument.Length > 3)
                        argument.Name = string.Join("_", splitArgument, 2, splitArgument.Length - 2);
                    else
                        argument.Name = splitArgument[2];

                    argument.Type = string.Join(" ", splitArgument, 0, 2);
                }
            }
            // Parameter is a TMap //
            else if (argumentString.Contains("TMap"))
            {
                // TMap format ( struct, struct ) //
                if (argumentString.Contains("<struct") && argumentString.Contains(", struct"))
                {
                    if (splitArgument.Length > 6)
                        argument.Name = string.Join("_", splitArgument, 6, splitArgument.Length - 6);
                    else
                        argument.Name = splitArgument[5];

                    argument.Type = string.Join(" ", splitArgument, 0, 5);
                }
                // TMap format ( struct, enum ) //
                else if (argumentString.Contains("<struct") && argumentString.Contains(", enum"))
                {
                    if (splitArgument.Length > 7)
                        argument.Name = string.Join("_", splitArgument, 6, splitArgument.Length - 6);
                    else
                        argument.Name = splitArgument[6];

                    argument.Type = string.Join(" ", splitArgument, 0, 6);
                }
                // TMap format ( struct, generic ) //
                else if (argumentString.Contains("<struct"))
                {
                    if (splitArgument.Length > 5)
                        argument.Name = string.Join("_", splitArgument, 4, splitArgument.Length - 4);
                    else
                        argument.Name = splitArgument[4];

                    argument.Type = string.Join(" ", splitArgument, 0, 4);
                }
                // TMap format ( enum, enum ) //  
                else if (argumentString.Contains("<enum") && argumentString.Contains(", enum"))
                {
                    if (splitArgument.Length > 8)
                        argument.Name = string.Join("_", splitArgument, 8, splitArgument.Length - 8);
                    else
                        argument.Name = splitArgument[7];

                    argument.Type = string.Join(" ", splitArgument, 0, 7);
                }
                // TMap format ( enum, struct ) //
                else if (argumentString.Contains("<enum") && argumentString.Contains(", struct"))
                {
                    if (splitArgument.Length > 7)
                        argument.Name = string.Join("_", splitArgument, 6, splitArgument.Length - 6);
                    else
                        argument.Name = splitArgument[6];

                        argument.Type = string.Join(" ", splitArgument, 0, 6);
                }
                // TMap format ( enum, generic ) //
                else if (argumentString.Contains("<enum"))
                {
                    if (splitArgument.Length > 6)
                        argument.Name = string.Join("_", splitArgument, 5, splitArgument.Length - 5);
                    else
                        argument.Name = splitArgument[5];

                    argument.Type = string.Join(" ", splitArgument, 0, 5);
                }
                // TMap format ( generic, struct ) //
                else if (argumentString.Contains(", struct"))
                {
                    if (splitArgument.Length > 5)
                        argument.Name = string.Join("_", splitArgument, 4, splitArgument.Length - 4);
                    else
                        argument.Name = splitArgument[4];

                    argument.Type = string.Join(" ", splitArgument, 0, 4);
                }
                // TMap format ( generic, enum ) //
                else if (argumentString.Contains(", enum"))
                {
                    string returnValue = string.Join(" ", splitArgument, 0, 5);

                    if (splitArgument.Length > 6)
                        argument.Name = string.Join("_", splitArgument, 5, splitArgument.Length - 5);
                    else
                        argument.Name = splitArgument[5];

                    argument.Type = string.Join(" ", splitArgument, 0, 5);
                }
                // TMap format ( generic, generic ) //
                else
                {
                    if (splitArgument.Length > 4)
                        argument.Name = string.Join("_", splitArgument, 3, splitArgument.Length - 3);
                    else
                        argument.Name = splitArgument[3];

                    argument.Type = string.Join(" ", splitArgument, 0, 3);
                }
            }
            // Parameter is struct, enum, or generic //
            else
            {
                if (argumentString.Contains("enum class"))
                {
                    if (splitArgument.Length > 4)
                        argument.Name = string.Join("_", splitArgument, 3, splitArgument.Length - 3);
                    else
                        argument.Name = splitArgument[3];

                    argument.Type = string.Join(" ", splitArgument, 0, 3);
                }
                else if (argumentString.Contains("struct "))
                {                   
                    if (splitArgument.Length > 3)
                        argument.Name = string.Join("_", splitArgument, 2, splitArgument.Length - 2);
                    else
                        argument.Name = splitArgument[2];

                    argument.Type = string.Join(" ", splitArgument, 0, 2);
                }
                else
                {
                    if(argumentString.Contains(" : "))
                    {
                        string leftOfColon = argumentString.Split(" : ")[0];
                        string rightOfColon = argumentString.Split(" : ")[1];

                        splitArgument = leftOfColon.Split(" ");

                        if (splitArgument.Length > 2)
                            argument.Name = string.Join("_", splitArgument, 1, splitArgument.Length - 2) + " : " + rightOfColon;
                        else
                            argument.Name = splitArgument[1] + " : " + rightOfColon;

                        argument.Type = splitArgument[0];
                    }
                    else
                    {
                        if (splitArgument.Length > 2)
                            argument.Name = string.Join("_", splitArgument, 1, splitArgument.Length - 1);
                        else
                            argument.Name = splitArgument[1];

                        argument.Type = splitArgument[0];
                    }
                }
            }

            argument.Name = ReplaceStrings(argument.Name);

            return argument;
        }
    }
}
