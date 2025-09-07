using Flurl;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime;

namespace BililiveRecorder.Common.Scripting.Runtime
{
    internal class JintURLSearchParams
    {
        private readonly QueryParamCollection query;

        public JintURLSearchParams(QueryParamCollection query)
        {
            this.query = query;
        }

        public JintURLSearchParams(JsValue jsValue)
        {
            if (jsValue.IsObject())
            {
                this.query = new QueryParamCollection();
                var obj = jsValue.AsObject();
                foreach (var p in obj.GetOwnProperties())
                {
                    this.query.Add(p.Key.ToString(), p.Value.Value.ToString());
                }
            }
            else
            {
                this.query = new QueryParamCollection(TypeConverter.ToString(jsValue));
            }
        }

        public void Append(string name, string value) => this.query.Add(name, value, nullValueHandling: NullValueHandling.NameOnly);
        public void Delete(string name) => this.query.Remove(name);
        public string?[][] Entries() => this.query.Select(x => new string?[] { x.Name, x.Value.ToString() }).ToArray();

        public void ForEach(FunctionInstance callback, JsValue thisArg)
        {
            var entries = this.Entries();
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                callback.Engine.Invoke(callback, thisArg, entry[1], entry[0], this);
            }
        }

        public string? Get(string name) => this.query.TryGetFirst(name, out var value) ? value.ToString() : null;
        public string?[] GetAll(string name) => this.query.GetAll(name).Select(x => x.ToString()).ToArray();
        public bool Has(string name) => this.query.Contains(name);
        public string[] Keys() => this.query.Select(x => x.Name).ToArray();
        public void Set(string name, string value) => this.query.AddOrReplace(name, value, nullValueHandling: NullValueHandling.NameOnly);

        public void Sort()
        {
            // do nothing
        }

        public override string ToString() => this.query.ToString();
        public string?[] Values() => this.query.Select(x => x.Value.ToString()).ToArray();
    }
}
