using Evernote.EDAM.NoteStore;
using Evernote.EDAM.Type;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Taggify.Model;

namespace Taggify.Controller
{
    public class TagController
    {
        private Auth _auth;
        private NoteStore.Client _store;
        private List<Tag> _cachedTags;
        private StringBuilder _log;
        private Hashtable _backupNotes;

        public TagController(Auth auth, NoteStore.Client store)
        {
            _auth = auth;
            _store = store;
            _cachedTags = GetTags();
            _backupNotes = new Hashtable();
            _log = new StringBuilder();
        }

        public List<Tag> GetTags()
        {
            return _store.listTags(_auth.token);
        }

        public List<Note> Taggify(List<Note> notes)
        {
            try
            {
                _log = new StringBuilder();
                int count = 0;
                List<Note> updatedNotes = new List<Note>();
                foreach (var nt in notes)
                {
                    List<string> noteTags = nt.TagGuids;
                    List<string> addedTags = new List<string>();
                    List<string> addedNameTags = new List<string>();
                    if (noteTags != null && noteTags.Count == 1)
                    {
                        foreach (var guid in noteTags)
                        {
                            List<Tag> parent_tags = GetParent(guid);
                            foreach (var parent in parent_tags)
                            {
                                if (!nt.TagGuids.Contains(parent.Guid))
                                {
                                    addedTags.Add(parent.Guid);
                                    _backupNotes[nt.Guid] = new TaggifyNote { tags = addedTags, tagNames = addedNameTags, note = nt };
                                    addedNameTags.Add(parent.Name);
                                }
                            }
                        }
                        _log.AppendLine(string.Format("Added {0} tags to '{1}' note", "[" + string.Join(",", addedNameTags) + "]", nt.Title));
                        nt.TagGuids.AddRange(addedTags);
                        updatedNotes.Add(nt);
                    }
                    else
                    {
                        count += 1;
                    }
                }

                if (count == notes.Count)
                {
                    _log.AppendLine("No note was modified!");
                }
                else
                {
                    _log.AppendLine("Taggified!");
                }
                
                return updatedNotes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Note> Untaggify()
        {
            _log = new StringBuilder();
            List<Note> updatedNotes = new List<Note>();
            foreach (DictionaryEntry entry in _backupNotes)
            {
                var nt = (TaggifyNote)entry.Value;
                foreach (var tag in nt.tags)
                {
                    nt.note.TagGuids.Remove(tag);
                }
                updatedNotes.Add(nt.note);
                _log.AppendLine(string.Format("Removed {0} tags to '{1}' note", "[" + string.Join(",", nt.tagNames) + "]", nt.note.Title));
            }

            _backupNotes = new Hashtable();
            return updatedNotes;
        }

        public string GetLog()
        {
            return _log.ToString();
        }

        private List<Tag> GetParent(string tagGuid)
        {
            try
            {
                if (tagGuid == null)
                {
                    return new List<Tag>();
                }
                else
                {
                    Tag tag = _cachedTags.Where(w => w.Guid == tagGuid).First();
                    List<Tag> results = new List<Tag>();
                    if (tag.ParentGuid != null)
                    {
                        results.Add(_cachedTags.Where(w => w.Guid == tag.ParentGuid).First());
                        List<Tag> parents = GetParent(tag.ParentGuid);
                        if (parents.Count() > 0)
                        {
                            results.AddRange(parents);
                        }
                    }
                    return results;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
