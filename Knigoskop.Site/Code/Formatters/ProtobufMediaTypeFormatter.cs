using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ProtoBuf.Meta;

namespace Knigoskop.Site.Code.Formatters
{
    public class ProtobufMediaTypeFormatter : MediaTypeFormatter
    {
        public const string ContentType = "text/protobuf";
        private readonly TypeModel _model;

        public ProtobufMediaTypeFormatter() : this(null)
        {
        }

        public ProtobufMediaTypeFormatter(TypeModel model)
        {
            _model = model ?? RuntimeTypeModel.Default;
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(ContentType));
            this.AddQueryStringMapping("format", "protobuf", ContentType);
        }

        public override bool CanReadType(Type type)
        {
            return _model.IsDefined(type);
        }

        public override bool CanWriteType(Type type)
        {
            return _model.IsDefined(type);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContent contentHeaders,
                                                         IFormatterLogger formatterLogger)
        {
            // write as sync for now
            var taskSource = new TaskCompletionSource<object>();
            try
            {
                taskSource.SetResult(_model.Deserialize(stream, null, type));
            }
            catch (Exception ex)
            {
                taskSource.SetException(ex);
            }
            return taskSource.Task;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContent contentHeaders,
                                                TransportContext transportContext)
        {
            // write as sync for now
            var taskSource = new TaskCompletionSource<object>();
            try
            {
                _model.Serialize(stream, value);
                taskSource.SetResult(null);
            }
            catch (Exception ex)
            {
                taskSource.SetException(ex);
            }
            return taskSource.Task;
        }
    }
}