/*
	wizdl - Web Service GUI
    Copyright (C) 2008  Ajai Shankar

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Text;
using System.Collections;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;

using System.Xml;
using System.Xml.Schema;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Web.Services.Protocols;

using Microsoft.CSharp;

namespace WebServices.UI
{
	/// <summary>
	/// Summary description for CustomProxyGenerator.
	/// </summary>
	public class CustomProxyGenerator
	{
		private const string RootNamespace = "WS";
		private const string ArgTypeSuffix = "Arg";
		private const string FieldNameSuffix = "_";

		public static Assembly CreateAssembly(string url)
		{
			CodeCompileUnit ccu = ImportServiceDescription(url);

			Hashtable enumTypes = new Hashtable();

			foreach(CodeTypeDeclaration typedecl in ccu.Namespaces[0].Types) 
			{
				if(typedecl.IsEnum)
					enumTypes[typedecl.Name] = typedecl;
			}

			ArrayList webServices = new ArrayList();

			foreach(CodeTypeDeclaration typedecl in ccu.Namespaces[0].Types)
			{
				AddEditorAttributes(typedecl, enumTypes);
                
				foreach(CodeTypeReference @ref in typedecl.BaseTypes) 
				{
					if(@ref.BaseType == "System.Web.Services.Protocols.SoapHttpClientProtocol") 
					{
                        if (IsEndPointExisted(webServices) == false)
                        {
                            webServices.Add(typedecl);
                            break;
                        }
                    }
				}
			}

			foreach(CodeTypeDeclaration svcdecl in webServices)
			{
				AddWebServiceMethodArgTypes(ccu.Namespaces[0], svcdecl, enumTypes);
			}

			return GenerateAssembly(ccu);
		}

		public static bool IsAsyncMethod(string methodName) 
		{
			return methodName.StartsWith("Begin") || methodName.StartsWith("End");
		}

		public static string GetMethodArgType(string methodName) 
		{
			return RootNamespace + "." + methodName + ArgTypeSuffix;
		}

		private static void AddWebServiceMethodArgTypes(CodeNamespace @namespace, CodeTypeDeclaration svcdecl, Hashtable enumTypes)
		{
			foreach(CodeTypeMember mem in svcdecl.Members)
			{
				if(!(mem is CodeMemberMethod))
					continue;

				CodeMemberMethod method = mem as CodeMemberMethod;

				if(method is CodeConstructor 
					|| method is CodeTypeConstructor
					|| IsAsyncMethod(method.Name))
				{
					continue;
				}

				CodeTypeDeclaration argtype = new CodeTypeDeclaration(method.Name + ArgTypeSuffix);

				CodeConstructor constructor = new CodeConstructor();

				constructor.Attributes = MemberAttributes.Public;
				
				argtype.Members.Add(constructor);				

				foreach(CodeParameterDeclarationExpression @param in method.Parameters) 
				{												
					argtype.Members.Add(new CodeMemberField(@param.Type, @param.Name));
				}

				AddEditorAttributes(argtype, enumTypes);

				@namespace.Types.Add(argtype);
			}
		}

		private static Assembly GenerateAssembly(CodeCompileUnit ccu)
		{		
			CSharpCodeProvider provider = new CSharpCodeProvider();

			ICodeCompiler compiler = provider.CreateCompiler();

			CompilerParameters options = new CompilerParameters(
				new string[] {
								 "System.dll",
								 "System.Drawing.dll",
								 "System.Design.dll",
								 "System.Data.dll",
								 "System.Web.Services.dll",
								 "System.Xml.dll",
								 typeof(CustomArrayEditor).Assembly.Location
							 }, string.Empty, false);										

			options.GenerateInMemory = false; // dynamic assemblies don't support xml serialization

			CompilerResults results = compiler.CompileAssemblyFromDom(options, ccu);


			StringBuilder errors = new StringBuilder();

			foreach(CompilerError error in results.Errors) 
			{
				if(!error.IsWarning) 
				{
					errors.Append(error.ErrorText);
					errors.Append(Environment.NewLine);
				}
			}

			if(errors.Length > 0)
				throw new Exception(errors.ToString());

			return Assembly.LoadFrom(results.PathToAssembly);
		}

		private static void AddEditorAttributes(CodeTypeDeclaration typedecl, Hashtable enumTypes)
		{
			if(typedecl.IsEnum)
				return;

			typedecl.CustomAttributes.Add(
				new CodeAttributeDeclaration(
					"System.ComponentModel.TypeConverterAttribute",
					new CodeAttributeArgument(
						new CodeTypeOfExpression(typeof(ExpandableObjectConverter)))));

			ArrayList fields = new ArrayList();

			CodeConstructor constructor = null;

			foreach(CodeTypeMember mem in typedecl.Members)
			{
				if (mem is CodeConstructor)
				{
					constructor = mem as CodeConstructor;
				}
				else if(mem is CodeMemberMethod && !IsAsyncMethod(mem.Name))
				{
					mem.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(TraceExtension).FullName));
				}
				else if(mem is CodeMemberField) 
				{
					fields.Add(mem);
				}
			}

			if (constructor == null)
			{
				constructor = new CodeConstructor();
				typedecl.Members.Add(constructor);
			}

			constructor.Attributes = MemberAttributes.Public;

			foreach(CodeMemberField field in fields)
			{
				// promote field to property
				CodeMemberProperty property = new CodeMemberProperty();

				property.Name = field.Name;
				property.Type = field.Type;
				property.Attributes = MemberAttributes.Public;
				property.CustomAttributes.AddRange(field.CustomAttributes);				

				// change field name, make private and clear custom attributes
				field.Name = field.Name + FieldNameSuffix;
				field.Attributes = MemberAttributes.Private;
				field.CustomAttributes.Clear();

				CodeTypeReference type = field.Type;

				// create zero length array so that ArrayEditor works properly
				if (type.ArrayRank == 1)
				{
					constructor.Statements.Add(
						new CodeAssignStatement(
							new CodeFieldReferenceExpression(
								new CodeThisReferenceExpression(), field.Name),
									new CodeArrayCreateExpression(type, 0)));
				}
				else if (type.ArrayRank > 1)
				{
					throw new Exception("Only single dimensional arrays supported");
				}
				
				property.GetStatements.Add(
					new CodeMethodReturnStatement(
						new CodeFieldReferenceExpression(
							new CodeThisReferenceExpression(), field.Name)));

				property.SetStatements.Add(
					new CodeAssignStatement(
						new CodeFieldReferenceExpression(
							new CodeThisReferenceExpression(), field.Name),
							new CodePropertySetValueReferenceExpression()));

				typedecl.Members.Add(property);				

				if(!type.BaseType.StartsWith("System") && !enumTypes.ContainsKey(type.BaseType))
				{
					if (type.ArrayRank == 0)
					{
						// [EditorAttribute(typeof(MyPropertyEditor), typeof(System.Drawing.Design.UITypeEditor))]        
						property.CustomAttributes.Add(
							new CodeAttributeDeclaration("System.ComponentModel.EditorAttribute",
							new CodeAttributeArgument[] {
								new CodeAttributeArgument(
									new CodeTypeOfExpression(typeof(CustomPropertyEditor))),
								new CodeAttributeArgument(
									new CodeTypeOfExpression(typeof(UITypeEditor))),
						}));
					}
					else
					{
						// [EditorAttribute(typeof(MyArrayEditor), typeof(System.Drawing.Design.UITypeEditor))]
						property.CustomAttributes.Add(
							new CodeAttributeDeclaration("System.ComponentModel.EditorAttribute",
							new CodeAttributeArgument[] {
								new CodeAttributeArgument(
									new CodeTypeOfExpression(typeof(CustomArrayEditor))),
								new CodeAttributeArgument(
									new CodeTypeOfExpression(typeof(UITypeEditor))),
						}));
					}
				}
			}
		}

		private static CodeCompileUnit ImportServiceDescription(string url)
		{
			ServiceDescriptionImporter importer = new ServiceDescriptionImporter();
			importer.ProtocolName = "Soap";
			importer.Style = ServiceDescriptionImportStyle.Client;            
			
			DiscoveryClientProtocol dcc = new DiscoveryClientProtocol();
			dcc.DiscoverAny(url);
			dcc.ResolveAll();

			foreach (object doc in dcc.Documents.Values)
			{					
				if(doc is ServiceDescription)
					importer.AddServiceDescription(doc as ServiceDescription, string.Empty, string.Empty);

				else if(doc is XmlSchema)
					importer.Schemas.Add(doc as XmlSchema);
			}

			if (importer.ServiceDescriptions.Count == 0) 
			{
				throw new Exception("No WSDL document was found at the url " + url);
			}

			CodeCompileUnit ccu = new CodeCompileUnit();
			
			ccu.Namespaces.Add(new CodeNamespace(RootNamespace));

			ServiceDescriptionImportWarnings warnings = importer.Import(ccu.Namespaces[0], ccu);

			if ((warnings & ServiceDescriptionImportWarnings.NoCodeGenerated) > 0) 
			{
				throw new Exception("No code generated");
			}

			return ccu;			
		}

        private static bool IsEndPointExisted(ArrayList webServices)
        {
            bool isExisted = false;
            foreach (CodeTypeDeclaration typedecl in webServices)
            {
                foreach (CodeTypeReference @ref in typedecl.BaseTypes)
                {
                    if (@ref.BaseType == "System.Web.Services.Protocols.SoapHttpClientProtocol")
                    {
                        isExisted = true;
                        break;
                    }
                }
            }
            return isExisted;

        }
	}
}
