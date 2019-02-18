using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Xml;

namespace SimpleAudioPlayer
{
    /// <summary>ファイルへのシリアライズヘルパー</summary>
    /// <typeparam name="T">シリアライズする型</typeparam>
    class SerializeHelper<T> where T : class, new()
    {
        private string appName;
        private string xmlPath;

        /// <summary>ファイルへのシリアライズヘルパー</summary>
        /// <param name="appName">ファイル先頭に書き込むコメント中の名前 特にチェックはしない デフォルトAssemblyName</param>
        public SerializeHelper(string appName = null)
        {
            this.appName = appName ?? Assembly.GetEntryAssembly().GetName().Name;
        }
        /// <summary>ファイルからデシリアライズ</summary>
        /// <param name="path">nullの場合 アプリ自体のpathから.exeを.configに変えたもの</param>
        public T Load(string path = null)
        {
            path = path ?? GetConfigFilePath();
            xmlPath = path;
            try
            {
                var serializer = new DataContractSerializer(typeof(T));
                using(var xr = XmlReader.Create(path))
                    return (T)serializer.ReadObject(xr);
            }
            catch(FileNotFoundException)
            {
                return new T();
            }
            catch(Exception e)
            {
                var msg = $"{appName}設定ファイルが読み込めません。\n"
                        + "初期設定で読み込みますか？（設定ファイルは上書きされます。）\n"
                        + path + "\n\n" + e.Message;
                var title = appName == null ? "エラー" : $"エラー - {appName}";
                var result = MessageBox.Show(msg, title, MessageBoxButton.YesNo, MessageBoxImage.Error);
                if(result != MessageBoxResult.Yes)
                    throw;

                return new T();
            }
        }
        private string GetConfigFilePath()
        {
            var path = Assembly.GetExecutingAssembly().Location;
            var dir = Path.GetDirectoryName(path);
            var name = Path.GetFileNameWithoutExtension(path);
            return Path.Combine(dir, name + ".config");
        }
        /// <summary>ファイルにシリアライズ Load前にSave不可</summary>
        /// <param name="displaySaveError">保存失敗時にダイアログを出すかどうか true:出す</param>
        public void Save(T obj, bool displaySaveError = true)
        {
            if(xmlPath == null) throw new InvalidOperationException("Load前にSaveはできません。");
            SaveAs(obj, xmlPath);
        }
        /// <summary>名前を付けてファイルにシリアライズ</summary>
        /// <param name="displaySaveError">保存失敗時にダイアログを出すかどうか true:出す</param>
        public void SaveAs(T obj, string path, bool displaySaveError = true)
        {
            var sb = new StringBuilder();
            sb.Append($"<!-- {appName} SettingFile Don't Edit. -->\n");
            try
            {
                StringWriter sw = null;
                try
                {
                    sw = new StringWriter(sb);
                    var settings = new XmlWriterSettings()
                    {
                        Indent = true,
                        OmitXmlDeclaration = true
                    };
                    using(var xw = XmlWriter.Create(sw, settings))
                    {
                        sw = null;
                        var serializer = new DataContractSerializer(typeof(T));
                        serializer.WriteObject(xw, obj);
                    }
                }
                finally
                {
                    if(sw != null)
                        sw.Dispose();
                }

                using(var stream = new StreamWriter(path, false, Encoding.UTF8))
                {
                    sb = sb.Replace(" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                    stream.Write(sb);
                }
            }
            catch(Exception e)
            {
                if(!displaySaveError)
                    return;
                var msg = $"{appName}設定ファイルが書き込めません。\n"
                    + "設定ファイルが読み取り専用になっていないか、またはアクセス許可があるフォルダにインストールされているかを確認してください。\n"
                    + "\n設定を保存せずに続行します。\n"
                    + "\n\n" + e.Message;
                var title = appName == null ? "エラー" : $"エラー - {appName}";
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
