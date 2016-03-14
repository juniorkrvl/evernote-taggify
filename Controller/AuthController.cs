using Evernote.EDAM.Error;
using Evernote.EDAM.NoteStore;
using Evernote.EDAM.UserStore;
using EvernoteOAuthNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taggify.Model;
using Taggify.Properties;
using Thrift.Protocol;
using Thrift.Transport;

namespace Taggify.Controller
{
    public class AuthController
    {
        public Auth Authenticate(EvernoteOAuth.HostService host = EvernoteOAuth.HostService.Sandbox)
        {
            try
            {
                //Insert you api key and secret on App.config file
                Auth auth = new Auth();

                string key = Properties.Settings.Default.key;
                string secret = Properties.Settings.Default.secret;

                EvernoteOAuth oauth = new EvernoteOAuth(host, key, secret);
                string errResponse = oauth.Authorize();
                if (errResponse.Length == 0)
                {
                    auth.noteStoreUrl = oauth.NoteStoreUrl;
                    auth.token = oauth.Token;
                    Settings set = Settings.Default;
                    set.token = oauth.Token;
                    set.Save();
                    auth.userId = oauth.UserId;
                    return auth;
                }
                else
                {
                    throw new ApplicationException("An error occurred while trying to connect with Evernote: " + errResponse);
                }
            }
            catch (EDAMSystemException edamEx)
            {
                throw edamEx;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while trying to connect with Evernote: ", ex);
            }
        }

        public Auth Connect(string token, EvernoteOAuth.HostService host = EvernoteOAuth.HostService.Sandbox)
        {
            try
            {
                //sandbox token
                //token = "S=s1:U=92214:E=15a51fa957a:C=152fa4968c8:P=185:A=juniorc:V=2:H=108ca178c3d91ad76115980cc6c4bce3";
                //official token
                //token = "S=s336:U=339313c:E=15a525c0e36:C=152faaae1e8:P=1cd:A=en-devtoken:V=2:H=639092783c184e3775697f3ee34a5c54";
                Auth auth = new Auth();

                string evernoteHost = "sandbox.evernote.com";
                if (host == EvernoteOAuth.HostService.Sandbox)
                {
                    evernoteHost = "sandbox.evernote.com";
                }
                else if (host == EvernoteOAuth.HostService.Production)
                {
                    evernoteHost = "www.evernote.com";
                }

                Uri userStoreUrl = new Uri("https://" + evernoteHost + "/edam/user");
                TTransport userStoreTransport = new THttpClient(userStoreUrl);
                TProtocol userStoreProtocol = new TBinaryProtocol(userStoreTransport);
                UserStore.Client userStore = new UserStore.Client(userStoreProtocol);

                auth.noteStoreUrl = userStore.getNoteStoreUrl(token);
                auth.token = token;

                return auth;
            }
            catch (EDAMSystemException edamEx)
            {
                throw edamEx;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while trying to connect with Evernote: ", ex);
            }
        }
    }
}
