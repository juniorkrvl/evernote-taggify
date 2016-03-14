using Evernote.EDAM.NoteStore;
using Evernote.EDAM.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taggify.Model;
using Thrift.Protocol;
using Thrift.Transport;

namespace Taggify.Controller
{
    public class NotebookController
    {
        private Auth _auth;
        private NoteStore.Client _store;

        public NotebookController(Auth auth, NoteStore.Client store)
        {
            _auth = auth;
            _store = store;
        }

        public List<Notebook> GetNotebooks()
        {
            try
            {
                return _store.listNotebooks(_auth.token);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Notebook GetNotebook(string guid)
        {
            try
            {
                return _store.getNotebook(_auth.token, guid);
            }
            catch (Exception)
            {
                throw;
            }
        }

       

    }
}
