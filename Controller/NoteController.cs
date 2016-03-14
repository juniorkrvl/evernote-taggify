using Evernote.EDAM.NoteStore;
using Evernote.EDAM.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taggify.Model;

namespace Taggify.Controller
{
    public class NoteController
    {
        private Auth _auth;
        private NoteStore.Client _store;

        public NoteController(Auth auth, NoteStore.Client store)
        {
            _auth = auth;
            _store = store;
        }

        public List<Note> GetNotes(string notebookGuid)
        {
            try
            {
                List<Note> listNotes = new List<Note>();

                NoteFilter filter = new NoteFilter();
                filter.NotebookGuid = notebookGuid;
                filter.Ascending = false;

                NoteList notes = _store.findNotes(_auth.token, filter, 0, 1);

                int totalNotes = 0;
                while (totalNotes < notes.TotalNotes)
                {
                    notes = _store.findNotes(_auth.token, filter, totalNotes, 200);
                    totalNotes += notes.Notes.Count();
                    listNotes.AddRange(notes.Notes);
                }
                return listNotes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateNotes(List<Note> list)
        {
            try
            {
                foreach (var note in list)
                {
                    _store.updateNote(_auth.token, note);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}