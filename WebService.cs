using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Web.Services.Protocols;

namespace WebServices.UI
{
    public class WebService : IComparable
    {
        private readonly SoapHttpClientProtocol _svc;
        private WebMethod[] _methods;

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
            foreach (WebMethod method in Methods)
            {
                if (string.Compare(method.Name, name, true) == 0)
                {
                    return method;
                }
            }
            return null;
        }

        public int CompareTo(object obj)
        {
            WebService other = obj as WebService;
            return Name.CompareTo(other.Name);
        }

        public override string ToString()
        {
            return _svc.GetType().Name;
        }

        public static WebService[] GetServices(string url)
        {
            ArrayList services = new ArrayList();

            Assembly assembly = CustomProxyGenerator.CreateAssembly(url);

            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsSubclassOf(typeof (SoapHttpClientProtocol)))
                {
                    continue;
                }

                SoapHttpClientProtocol svc = Activator.CreateInstance(type) as SoapHttpClientProtocol;

                if (url.ToLower().EndsWith("?wsdl"))
                {
                    svc.Url = url.Substring(0, url.Length - 5);
                }

                services.Add(new WebService(svc));
            }

            services.Sort();

            return services.ToArray(typeof (WebService)) as WebService[];
        }

        private void InitializeMethods()
        {
            ArrayList list = new ArrayList();

            Type type = _svc.GetType();

            foreach (MethodInfo method in type.GetMethods(
                BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance))
            {
                if (CustomProxyGenerator.IsAsyncMethod(method.Name))
                    continue;

                if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
                    continue;

                list.Add(new WebMethod(_svc, method));
            }

            list.Sort();

            _methods = list.ToArray(typeof (WebMethod)) as WebMethod[];
        }
    }

    [DefaultProperty("Arg")]
    public class WebMethod : IComparable
    {
        private readonly SoapHttpClientProtocol _svc;
        private readonly MethodInfo _methodInfo;
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
        [TypeConverter(typeof (ExpandableObjectConverter))]
        public SoapHttpClientProtocol ConfigureService
        {
            get { return _svc; }
        }

        [Browsable(true)]
        [TypeConverter(typeof (ExpandableObjectConverter))]
        public object ConfigureArguments
        {
            get { return _arg; }
        }

        public override string ToString()
        {
            return Name;
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

            for (int i = 0; i < @params.Length; ++i)
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