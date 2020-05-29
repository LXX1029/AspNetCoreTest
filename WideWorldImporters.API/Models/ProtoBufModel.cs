using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using ProtoBuf;

namespace WideWorldImporters.API.Models
{
    [ProtoContract]
    public class ProtoBufModel
    {
        [ProtoMember(1)]
        public int Id { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
    }

    public class ProtoBufFormatter : OutputFormatter
    {
        private string ContentType { get; set; }
        public ProtoBufFormatter()
        {
            ContentType = "application/proto";
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/proto"));
            
        }



        public override  Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            var response = context.HttpContext.Response;
            Serializer.Serialize(response.Body, context.Object);
            return Task.FromResult(0);
        }
    }
}
