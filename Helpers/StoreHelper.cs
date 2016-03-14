using Evernote.EDAM.NoteStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taggify.Model;
using Thrift.Protocol;
using Thrift.Transport;

namespace Taggify.Helpers
{
    public static class StoreHelper
    {
        public static NoteStore.Client CreateNoteStore(Auth auth)
        {
            try
            {
                TTransport noteStoreTransport = new THttpClient(new Uri(auth.noteStoreUrl));
                TProtocol noteStoreProtocol = new TBinaryProtocol(noteStoreTransport);
                return new NoteStore.Client(noteStoreProtocol);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
