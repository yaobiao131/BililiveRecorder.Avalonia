using System.Collections.Specialized;
using System.Net;
using System.Web;
using Jint;
using Jint.Native.Object;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace BililiveRecorder.Common.Scripting.Runtime
{
    internal class JintDotnet : ObjectInstance
    {
        public JintDotnet(Engine engine) : base(engine)
        {
        }

        protected override void Initialize()
        {
            this.FastSetProperty("Dns", new PropertyDescriptor(TypeReference.CreateTypeReference(this._engine, typeof(Dns)), false, false, false));

            AddTypeAsProperty<Uri>();
            AddTypeAsProperty<UriBuilder>();
            AddTypeAsProperty<HttpUtility>();
            AddTypeAsProperty<NameValueCollection>();

            AddTypeAsProperty<HttpClient>();
            AddTypeAsProperty<HttpClientHandler>();
            AddTypeAsProperty<HttpCompletionOption>();
            AddTypeAsProperty<HttpRequestMessage>();
            AddTypeAsProperty<HttpMethod>();
            AddTypeAsProperty<ByteArrayContent>();
            AddTypeAsProperty<StringContent>();
            AddTypeAsProperty<FormUrlEncodedContent>();

            void AddTypeAsProperty<T>() => this.FastSetProperty(typeof(T).Name, new PropertyDescriptor(TypeReference.CreateTypeReference<T>(this._engine), false, false, false));
        }
    }
}
