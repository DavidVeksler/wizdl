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
using System.ComponentModel;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace WebServices.UI
{
	public class WebService : IComparable
	{
		private SoapHttpClientProtocol _svc;
		private WebMethod[] _methods = null;

		private WebService(SoapHttpClientProtocol svc)
		{
			_svc = svc;
			InitializeMethods();
		}

		[Browsable(false)]
		public SoapHttpClientProtocol Service
		{
			get { return _svc; }
		}

		[Browsable(false)]
		public string Name
		{
			get { return _svc.GetType().Name; }
		}

		public WebMethod[] Methods 
		{
			get { return _methods; }
		}

		public WebMethod FindMethod(string name)
		{
			foreach(WebMethod method in this.Methods) 
			{
				if(string.Compare(method.Name, name, true) == 0) 
				{
					return method;
				}
			}
			return null;
		}

		public int CompareTo(object obj)
		{
			WebService other = obj as WebService;
			return this.Name.CompareTo(other.Name);
		}

		public override string ToString()
		{
			return _svc.GetType().Name;
		}

		public static WebService[] GetServices(string url)
		{
			ArrayList services = new ArrayList();

			Assembly assembly = CustomProxyGenerator.CreateAssembly(url);

			foreach(Type type in assembly.GetTypes()) 
			{
				if(!type.IsSubclassOf(typeof(SoapHttpClientProtocol)))
				{
					continue;
				}

				SoapHttpClientProtocol svc = Activator.CreateInstance(type) as SoapHttpClientProtocol;

				if(url.ToLower().EndsWith("?wsdl")) 
				{
					svc.Url = url.Substring(0, url.Length - 5);
				}

				services.Add(new WebService(svc));
			}

			services.Sort();

			return services.ToArray(typeof(WebService)) as WebService[];
		}

		private void InitializeMethods()
		{
			ArrayList list = new ArrayList();

			Type type = _svc.GetType();

			foreach(MethodInfo method in type.GetMethods(
				BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance))
			{
				if(CustomProxyGenerator.IsAsyncMethod(method.Name))
					continue;

				if(method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
					continue;

				list.Add(new WebMethod(_svc, method));
			}

			list.Sort();

			_methods = list.ToArray(typeof(WebMethod)) as WebMethod[];
		}
	}

	[DefaultProperty("Arg")]
	public class WebMethod : IComparable
	{		
		private SoapHttpClientProtocol _svc;
		private MethodInfo _methodInfo;
		private object _arg;

		internal WebMethod(SoapHttpClientProtocol svc, MethodInfo methodInfo) 
		{
			_svc = svc;
			_methodInfo = methodInfo;
			string argType = CustomProxyGenerator.GetMethodArgType(methodInfo.Name);
			_arg = Activator.CreateInstance(svc.GetType().Assembly.GetType(argType));
		}
		
		[Browsable(false)]
		public SoapHttpClientProtocol Service
		{
			get { return _svc; }
		}

		[Browsable(false)]
		public object Arg 
		{
			get { return _arg; }
		}

		[Browsable(false)]
		public string Name
		{
			get { return _methodInfo.Name; }
		}

		[Browsable(true)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public SoapHttpClientProtocol ConfigureService
		{
			get { return _svc; }
		}

		[Browsable(true)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public object ConfigureArguments 
		{
			get { return _arg; }
		}

		public override string ToString()
		{
			return this.Name;
		}

		internal void SetArg(object arg)
		{
			_arg = arg;
		}

		public object Invoke()
		{
			Type argType = _arg.GetType();
			ParameterInfo[] @params = _methodInfo.GetParameters();
				
			object[] args = new object[@params.Length];

			for(int i = 0; i < @params.Length; ++i) 
			{
				ParameterInfo pi = @params[i];
				args[i] = argType.GetProperty(pi.Name).GetValue(_arg, null);
			}
				
			return _methodInfo.Invoke(_svc, args);
		}

		public int CompareTo(object obj)
		{
			WebMethod other = obj as WebMethod;
			return _methodInfo.Name.CompareTo(other._methodInfo.Name);
		}
	}
}
