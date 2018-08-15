/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.IO;
using System.Text;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell;
using VSLangProj80;

namespace Knacka.Se.ProtobufGenerator
{
    /// <summary>
    /// This is the generator class. 
    /// When setting the 'Custom Tool' property of a C#, VB, or J# project item to "XmlClassGenerator", 
    /// the GenerateCode function will get called and will return the contents of the generated file 
    /// to the project system
    /// </summary>
    [ComVisible(true)]
    [Guid("10B050D0-1362-4692-A351-2BB29F63F712")]
    [CodeGeneratorRegistration(typeof(ProtobufGenerator), "ProtobufGenerator - Generate C# from proto files", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true)]
    [CodeGeneratorRegistration(typeof(ProtobufGenerator), "ProtobufGenerator - Generate C# from proto files", "{7B2BD99C-EC9E-47DA-A1E4-77F8A1234}", GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof(ProtobufGenerator))]
    public class ProtobufGenerator : BaseCodeGeneratorWithSite
    {
#pragma warning disable 0414
        //The name of this generator (use for 'Custom Tool' property of project item)
        internal static string name = "ProtobufGenerator";
#pragma warning restore 0414

        private static string _protocPath;

        /// <summary>
        /// Function that builds the contents of the generated file based on the contents of the input file
        /// </summary>
        /// <param name="inputFileContent">Content of the input file</param>
        /// <returns>Generated file as a byte array</returns>
        protected override byte[] GenerateCode(string inputFileContent)
        {
            var vsItem = this.GetVSProjectItem();
            var name = vsItem?.ProjectItem?.Name;
            var path = vsItem?.ProjectItem?.Document?.Path;

            if (string.IsNullOrEmpty(_protocPath))
            {
                IHaveProtoc protocFinder = new HaveProtoc(path);
                if (protocFinder.HaveFoundProtoc)
                    _protocPath = protocFinder.ProtocPath;
            }

            if (string.IsNullOrEmpty(_protocPath))
            {
                this.GeneratorError(4, "Protoc.exe not found. Please read the documentation for ProtobufGenerator", 1, 1);
                return null;
            }

            ICanGenerateFromProto generator = new GenerateFromProto(_protocPath);

            try
            {
                if (this.CodeGeneratorProgress != null)
                {
                    this.CodeGeneratorProgress.Progress(50, 100);
                }
                var res = generator.GenerateCsharpFromProto(inputFileContent, path, name);
                if (this.CodeGeneratorProgress != null)
                {
                    this.CodeGeneratorProgress.Progress(100, 100);
                }
                return res;
            }
            catch (Exception e)
            {
                this.GeneratorError(4, e.ToString(), 1, 1);
                //Returning null signifies that generation has failed
                return null;
            }
        }

        
    }
}