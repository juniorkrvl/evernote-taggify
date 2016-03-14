using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EvernoteOAuthNet;
using Thrift;
using Evernote.EDAM;
using Thrift.Transport;
using Thrift.Protocol;
using Evernote.EDAM.NoteStore;
using Evernote.EDAM.Type;
using Evernote.EDAM.UserStore;
using Taggify.Controller;
using Taggify.Model;
using Taggify.Properties;
using Evernote.EDAM.Error;
using System.Reflection;
using System.Linq.Expressions;
using Taggify.Helpers;
using Squirrel;
using System.Threading;

namespace Taggify
{
    public partial class MainView : Form
    {
        private Auth _auth;
        private NoteStore.Client _store;
        private NotebookController _notebookController;
        private TagController _tagController;

        public MainView()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                AuthController auth = new AuthController();
                _auth = auth.Authenticate(EvernoteOAuth.HostService.Production);
                if (_auth != null)
                {
                    InitialSetup();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void EnableControllers()
        {
            cbNotebooks.Enabled = true;
            btnTaggify.Enabled = true;
            txtLog.Enabled = true;
        }

        public void InitialSetup()
        {
            _store = StoreHelper.CreateNoteStore(_auth);
            _tagController = new TagController(_auth, _store);
            _notebookController = new NotebookController(_auth, _store);

            EnableControllers();
            FillComboBox();
        }

        private void FillComboBox()
        {
            try
            {
                List<Notebook> list = _notebookController.GetNotebooks();
                cbNotebooks.DataSource = list;
                cbNotebooks.DisplayMember = "Name";
                cbNotebooks.ValueMember = "Guid";

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Processing()
        {
            lblStatus.Text = "Processing...";
            Application.DoEvents();
            this.Cursor = Cursors.WaitCursor;
        }

        private void Done()
        {
            lblStatus.Text = "...";
            this.Cursor = Cursors.Default;
        }

        private void MainView_Load(object sender, EventArgs e)
        {
            try
            {
                if (Settings.Default.token != null && Settings.Default.token != "")
                {
                    AuthController authController = new AuthController();
                    _auth = authController.Connect(Settings.Default.token, EvernoteOAuth.HostService.Production);
                    if (_auth != null)
                    {
                        btnConnect.Hide();
                        InitialSetup();
                    }
                    else
                    {
                        btnConnect.Show();
                    }
                }

                new Thread(() =>
                {
                    UpdateApp();
                }).Start();

            }
            catch (EDAMSystemException edam)
            {
                if (edam.ErrorCode == EDAMErrorCode.AUTH_EXPIRED)
                {
                    MessageBox.Show("Taggify is no longer permitted to access your evernote account. Please log in again!");
                    btnConnect.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public async void UpdateApp()
        {
            try
            {
                string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                using (var mgr = UpdateManager.GitHubUpdateManager("https://github.com/juniorkrvl/evernote-taggify"))
                {
                    await mgr.Result.UpdateApp();
                }
            }
            catch (Exception)
            {
                txtLog.SetPropertyThreadSafe(() => txtLog.Text, "No new releases.");
            }
        }

        private void btnTaggify_Click(object sender, EventArgs e)
        {
            try
            {
                Processing();

                string guid = cbNotebooks.SelectedValue.ToString();
                btnTaggify.Enabled = false;

                Notebook notebook = _notebookController.GetNotebook(guid);
                if (notebook != null)
                {
                    NoteController noteController = new NoteController(_auth, _store);
                    List<Note> updatedNotes = _tagController.Taggify(noteController.GetNotes(notebook.Guid));
                    noteController.UpdateNotes(updatedNotes);
                    btnUntaggify.Enabled = true;
                    btnTaggify.Enabled = true;
                    txtLog.Text = _tagController.GetLog();
                }

                Done();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void btnUntaggify_Click(object sender, EventArgs e)
        {
            Processing();

            List<Note> notes = _tagController.Untaggify();
            if (notes.Count > 0)
            {
                NoteController noteController = new NoteController(_auth, _store);
                noteController.UpdateNotes(notes);
                txtLog.Text = _tagController.GetLog();
                txtLog.Text += "\nUntaggified!";
            }
            else
            {
                txtLog.Text += "No note was modified";
            }
            btnUntaggify.Enabled = false;

            Done();
        }

        private void btnSync_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Syncing...";
            Application.DoEvents();
            FillComboBox();
            lblStatus.Text = "...";
        }
    }
}
