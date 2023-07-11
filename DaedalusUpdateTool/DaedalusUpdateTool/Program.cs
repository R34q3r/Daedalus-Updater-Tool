using DaedalusUpdateTool;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Xml.Linq;
using static DaedalusUpdateTool.ModdingTools;

Console.Write("Dump Folder : ");
string dumpFolderLocation = Console.ReadLine();

Console.Clear();

Console.Write("Generated Files Folder : ");
string generatedFilesFolderPath = Console.ReadLine();

Console.Clear();

List<string> dumpedFilePaths = Directory.GetFiles(dumpFolderLocation).ToList();

List<ClassFile> allClasses = new List<ClassFile>();
List<string> addedClasses = new List<string>();

foreach (string dumpedFilePath in dumpedFilePaths)
{
    List<string> dumpedFileLines = File.ReadAllLines(dumpedFilePath).ToList();

    for(int i = 0; i < dumpedFileLines.Count; i++)
    {
        if (dumpedFileLines[i].Contains("struct ") && dumpedFileLines[i].Contains("{"))
        {
            ClassFile classFile = new ClassFile();

            string[] splitLine = dumpedFileLines[i].Split(" ");

            classFile.FileName = Path.GetFileName(dumpedFilePath);
            classFile.ClassName = splitLine[1];

            bool endOfClassFound = false;

            classFile.Lines.Add("LOADER_API " + dumpedFileLines[i]);

            if (splitLine.Contains(":"))
                classFile.ParentClass = splitLine[3];

            if (!ModdingTools.IsReplaced(dumpedFileLines[i]) && !dumpedFileLines[i + 1].Contains("struct FVector StartPoint_2_"))
            {
                i++;

                while (!endOfClassFound)
                {
                    classFile.Lines.Add(dumpedFileLines[i]);

                    // Line Contains a Class Variable //
                    if (!dumpedFileLines[i].Contains(" // Function ") && !dumpedFileLines[i].Contains(" // DelegateFunction "))
                    {
                        if (dumpedFileLines[i].Contains("}"))
                        {
                            endOfClassFound = true;

                            i++;

                            break;
                        }

                        if (ModdingTools.IsBanned(dumpedFileLines[i]))
                        {
                            List<string> reversedSplitLine = dumpedFileLines[i].Split(" ").Reverse().ToList();
                            string nameWithColon = reversedSplitLine[2];

                            string sizeWithParth = reversedSplitLine[0].Split("(")[1];

                            classFile.Lines[classFile.Lines.Count - 1] = "    char " + ModdingTools.ReplaceStrings(nameWithColon.Substring(0, nameWithColon.Length - 1)) + "[" + sizeWithParth.Substring(0, sizeWithParth.Length - 1) + "];";
                        }
                        else
                        {
                            if (!dumpedFileLines[i].Contains("char pad_"))
                            {
                                if (dumpedFileLines[i].Split(" // ")[0] != "")
                                {
                                    dumpedFileLines[i] = dumpedFileLines[i].Replace("* BuffName", "* _BuffName");

                                    Variable fixedArgument = ModdingTools.GetVariableValues(dumpedFileLines[i].Split(" // ")[0]);

                                    classFile.ChildClasses.Add(fixedArgument.Type);

                                    classFile.Lines[classFile.Lines.Count - 1] = fixedArgument.Type + " " + fixedArgument.Name;
                                }
                            }
                        }
                    }
                    // Line Contains a Function //
                    else if (classFile.Lines[classFile.Lines.Count - 1].Count(x => x == '(') == 2)
                    {
                        if (classFile.Lines[classFile.Lines.Count - 1].Contains("Function BP_Building_Base.BP_Building_Base_C.DecideShifting"))
                        {
                            Console.WriteLine(classFile.Lines[classFile.Lines.Count - 1]);
                        }

                        // Line Contains a Banned Word //
                        if (ModdingTools.IsBanned(classFile.Lines[classFile.Lines.Count - 1]))
                        {
                            classFile.Lines[classFile.Lines.Count - 1] = "//" + classFile.Lines[classFile.Lines.Count - 1];
                        }
                        // Line does not Contain a Banned Word //
                        else
                        {
                            Function function = new Function();

                            string returnSide = classFile.Lines[classFile.Lines.Count - 1].Split("(")[0];
                            string parameterSide = classFile.Lines[classFile.Lines.Count - 1].Split("(")[1];
                            string[] splitReturnSide = returnSide.Split(" ");

                            Variable argument = ModdingTools.GetVariableValues(returnSide);

                            function.Name = argument.Name;
                            function.ReturnType = argument.Type;

                            classFile.Lines[classFile.Lines.Count - 1] = "LOADER_API " +  function.ReturnType + " " + function.Name + "(" + classFile.Lines[classFile.Lines.Count - 1].Split("(")[1];

                            returnSide = classFile.Lines[classFile.Lines.Count - 1].Split("(")[0];

                            List<string> parameters = parameterSide.Split(")")[0].Split(",").ToList();

                            for (int i1 = 0; i1 < parameters.Count; i1++)
                            {
                                if (i1 < parameters.Count - 1)
                                {
                                    if (parameters[i1].Contains("TMap<") && parameters[i1 + 1].Contains(">"))
                                    {
                                        string joinedTMap = parameters[i1] + "," + parameters[i1 + 1];

                                        parameters.RemoveAt(i1 + 1);

                                        parameters[i1] = joinedTMap;
                                    }
                                }
                            }

                            string fixedParameters = "";

                            int index = 0;

                            parameters.RemoveAll(x => x == "");

                            foreach (string parameter in parameters)
                            {
                                Variable arg = ModdingTools.GetVariableValues(parameter);

                                if (index < parameters.Count - 1)
                                    fixedParameters += arg.Type + " " + arg.Name + ",";
                                else
                                    fixedParameters += arg.Type + " " + arg.Name;

                                function.Arguments.Add(arg);

                                index++;
                            }

                            string evenMoreFixedParameters = "";

                            int index1 = 0;

                            foreach (string parameter in fixedParameters.Split(","))
                            {
                                if (parameter != "" && parameter != " ")
                                {
                                    if (index1 < fixedParameters.Split(",").Length - fixedParameters.Split(",").Count(x => x == "") - 1)
                                        evenMoreFixedParameters += parameter + ",";
                                    else
                                        evenMoreFixedParameters += parameter;
                                }

                                index1++;
                            }

                            classFile.Lines[classFile.Lines.Count - 1] = returnSide + "(" + evenMoreFixedParameters + ")" + classFile.Lines[classFile.Lines.Count - 1].Split(")")[1];

                            function.FunctionPath = classFile.Lines[classFile.Lines.Count - 1].Split(" // ")[1];

                            classFile.Functions.Add(function);
                        }
                    }
                    else
                    {
                        classFile.Lines[classFile.Lines.Count - 1] = "//" + classFile.Lines[classFile.Lines.Count - 1];
                    }

                    i++;
                }

                allClasses.Add(classFile);
            }
            else
            {
                classFile.IsReal = false;
            }
        }
        // Replaces enumerable uint8 with uint8_t //
        else if (dumpedFileLines[i].Contains(" : uint8 {"))
        {
            if (!IsReplaced(dumpedFileLines[i]))
            {
                ClassFile classFile = new ClassFile();

                classFile.ClassName = dumpedFileLines[i].Split(" ")[2];
                classFile.FileName = Path.GetFileName(dumpedFilePath);

                classFile.IsEnum = true;

                bool foundEndOfClass = false;

                classFile.Lines.Add(dumpedFileLines[i].Replace("uint8", "uint8_t"));

                i++;

                while (!foundEndOfClass)
                {
                    classFile.Lines.Add(dumpedFileLines[i]);

                    if (dumpedFileLines[i].Contains("};"))
                        foundEndOfClass = true;
                    else if (dumpedFileLines[i].Contains("PF_MAX = 72"))
                        classFile.Lines[classFile.Lines.Count - 1] = "\t\tPF_MAXX = 72";

                    i++;
                }

                allClasses.Add(classFile);
            }
        }
        else if (dumpedFileLines[i].Contains(" : int32 {"))
        {
            if (!IsReplaced(dumpedFileLines[i]))
            {
                ClassFile classFile = new ClassFile();

                classFile.ClassName = dumpedFileLines[i].Split(" ")[2];
                classFile.FileName = Path.GetFileName(dumpedFilePath);

                classFile.IsEnum = true;

                bool foundEndOfClass = false;

                classFile.Lines.Add(dumpedFileLines[i].Replace("int32", "int32_t"));

                i++;

                while (!foundEndOfClass)
                {
                    if (dumpedFileLines[i].Contains("};"))
                        foundEndOfClass = true;

                    classFile.Lines.Add(dumpedFileLines[i]);

                    i++;
                }

                allClasses.Add(classFile);
            }
        }
    }
}

// Class sorting //
List<ClassFile> headerFile = allClasses.FindAll(x => !x.IsEnum);
List<string> textHeaderFile = new List<string>();
List<string> textSourceFile = new List<string>();

textSourceFile.AddRange(new List<string>
{
    "#include <sdk.h>",
    "",
    "namespace UE4",
    "{"
});

while (true)
{
    int errorCount = 0; ;

    for (int i = 0; i < headerFile.Count; i++)
    {
        // Parent handling //
        if (headerFile[i].ParentClass != "")
        {
            int parentIndex = headerFile.FindIndex(x => x.ClassName == headerFile[i].ParentClass);

            if(parentIndex != -1)
            {
                if(parentIndex > i)
                {
                    ClassFile cachedParent = headerFile[parentIndex];

                    headerFile.RemoveAt(parentIndex);
                    headerFile.Insert(i, cachedParent);

                    errorCount++;
                }
            }
        }

        // Child handling //
        foreach(string child in headerFile[i].ChildClasses)
        {
            int childIndex = headerFile.FindIndex(x => x.ClassName == GetTypeWithoutPre(child));

            if (childIndex != -1)
            {
                if (childIndex > i)
                {
                    ClassFile cachedChild = headerFile[childIndex];

                    headerFile.RemoveAt(childIndex);
                    headerFile.Insert(i, cachedChild);

                    errorCount++;
                }
            }
        }

        // Function handling //
        foreach(Function function in headerFile[i].Functions)
        {
            // Return value handling //
            int returnValueIndex = headerFile.FindIndex(x => x.ClassName == GetTypeWithoutPre(function.ReturnType));

            if (returnValueIndex != -1)
            {
                if (returnValueIndex > i)
                {
                    ClassFile cachedChild = headerFile[returnValueIndex];

                    headerFile.RemoveAt(returnValueIndex);
                    headerFile.Insert(i, cachedChild);

                    errorCount++;
                }
            }

            // Argument handling //
            foreach(Variable argument in function.Arguments)
            {
                int argumentIndex = headerFile.FindIndex(x => x.ClassName == GetTypeWithoutPre(argument.Type));

                if (argumentIndex != -1)
                {
                    if (argumentIndex > i)
                    {
                        ClassFile cachedChild = headerFile[argumentIndex];

                        headerFile.RemoveAt(argumentIndex);
                        headerFile.Insert(i, cachedChild);

                        errorCount++;
                    }
                }
            }
        }
    }

    Console.Clear();
    Console.WriteLine( "Sorting errors : " + errorCount.ToString());

    if (errorCount == 0)
        break;
}

// Enum handling //
headerFile.InsertRange(0, allClasses.FindAll(x => x.IsEnum));

//Function handling //
foreach (ClassFile classFile in allClasses)
{
    foreach (Function function in classFile.Functions)
    {
        function.Arguments.RemoveAll(x => x.Name == "" || x.Type == "");

        textSourceFile.Add(function.ReturnType + " " + classFile.ClassName + "::" + function.Name + "(");

        foreach (Variable arg in function.Arguments)
        {
            if (arg != function.Arguments.Last())
                textSourceFile[textSourceFile.Count - 1] += (arg.Type + " " + arg.Name) + ",";
            else
                textSourceFile[textSourceFile.Count - 1] += (arg.Type + " " + arg.Name);
        }

        textSourceFile[textSourceFile.Count - 1] += ")";

        textSourceFile.Add("    {");

        textSourceFile.Add("        UE4::UFunction* func = UE4::UObject::FindObject<UE4::UFunction>(\"" + function.FunctionPath + "\");");
        textSourceFile.Add("");

        textSourceFile.Add("        struct {");

        foreach (Variable arg in function.Arguments)
            textSourceFile.Add("            " + (arg.Type + " " + arg.Name).Replace("&", "") + ";");

        if (!function.ReturnType.Contains("void"))
            textSourceFile.Add("            " + (function.ReturnType.Trim() + " ReturnValue").Replace("&", "") + ";");

        textSourceFile.Add("        }params;");

        textSourceFile.Add("");

        foreach (Variable arg in function.Arguments)
            textSourceFile.Add("        params." + arg.Name + "=" + arg.Name + ";");

        textSourceFile.Add("");

        textSourceFile.Add("        this->ProcessEvent(func, &params);");

        if(!function.ReturnType.Contains("void"))
            textSourceFile.Add("        return params.ReturnValue;");

        textSourceFile.Add("    }");

        textSourceFile.Add("");
    }
}

// File generation //
textHeaderFile.AddRange(new List<string>
{
    "#include <CoreUObject_classes.hpp>",
    "",
    "namespace UE4",
    "{"
});

foreach (ClassFile classFile in headerFile)
{
    textHeaderFile.AddRange(classFile.Lines);
    textHeaderFile.Add("");
}

textHeaderFile.Add("};");
textSourceFile.Add("};");

File.Create(generatedFilesFolderPath + @"\" + "sdk.h").Close();
File.WriteAllLines(generatedFilesFolderPath + @"\" + "sdk.h", textHeaderFile);

File.Create(generatedFilesFolderPath + @"\" + "sdk.cpp").Close();
File.WriteAllLines(generatedFilesFolderPath + @"\" + "sdk.cpp", textSourceFile);

Console.WriteLine("Finished.");
Console.ReadLine();