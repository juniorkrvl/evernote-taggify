using Evernote.EDAM.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taggify.Model
{
    public class TaggifyNote
    {
        public List<string> tags { get; set; }
        public List<string> tagNames { get; set; }
        public Note note { get; set; }
    }
}
