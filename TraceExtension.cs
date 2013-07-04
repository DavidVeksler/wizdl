using System;
using System.IO;
using System.Web.Services.Protocols;
using System.Windows.Forms;

namespace WebServices.UI
{
    /// <summary>
    ///     Modified from MSDN sample, sends all output to a textbox
    /// </summary>
    public class TraceExtension : SoapExtension
    {
        private Stream oldStream;
        private Stream newStream;

        private static TextBox _textBox;

        internal static TextBox SoapTextBox
        {
            set { _textBox = value; }
        }

        public override Stream ChainStream(Stream stream)
        {
            oldStream = stream;
            newStream = new MemoryStream();
            return newStream;
        }

        public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute)
        {
            return null;
        }

        public override object GetInitializer(Type WebServiceType)
        {
            return null;
        }

        public override void Initialize(object initializer)
        {
        }

        public override void ProcessMessage(SoapMessage message)
        {
            switch (message.Stage)
            {
                case SoapMessageStage.BeforeSerialize:
                    break;
                case SoapMessageStage.AfterSerialize:
                    WriteRequest(message);
                    break;
                case SoapMessageStage.BeforeDeserialize:
                    WriteResponse(message);
                    break;
                case SoapMessageStage.AfterDeserialize:
                    break;
                default:
                    throw new Exception("invalid stage");
            }
        }

        public void WriteRequest(SoapMessage message)
        {
            newStream.Position = 0;
            if (_textBox != null)
            {
                _textBox.Text += string.Format("-----SoapRequest at {0}\r\n\r\n", DateTime.Now);
                _textBox.Text += new StreamReader(newStream).ReadToEnd() + "\r\n\r\n";
            }
            newStream.Position = 0;
            Copy(newStream, oldStream);
        }

        public void WriteResponse(SoapMessage message)
        {
            Copy(oldStream, newStream);
            newStream.Position = 0;
            if (_textBox != null)
            {
                _textBox.Text += string.Format("-----SoapResponse at {0}\r\n\r\n", DateTime.Now);
                _textBox.Text += new StreamReader(newStream).ReadToEnd() + "\r\n";
            }
            newStream.Position = 0;
        }

        private static void Copy(Stream from, Stream to)
        {
            TextReader reader = new StreamReader(from);
            TextWriter writer = new StreamWriter(to);
            writer.WriteLine(reader.ReadToEnd());
            writer.Flush();
        }
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class TraceExtensionAttribute : SoapExtensionAttribute
    {
        public override Type ExtensionType
        {
            get { return typeof (TraceExtension); }
        }

        public override int Priority { get; set; }
    }
}