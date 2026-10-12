using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace KoSch.SchoolTycoon.Core
{
    /// <summary>Six isolated saves with checksums, flushed temporary writes and recoverable backups. Serializer supplied by the target.</summary>
    public sealed class SaveArchive
    {
        public const int Slots=6,MaxBytes=64*1024*1024;
        private readonly string directory;private readonly Func<SchoolState,string> encode;private readonly Func<string,SchoolState> decode;
        public SaveArchive(string folder,Func<SchoolState,string> serializer,Func<string,SchoolState> deserializer)
        {directory=folder??throw new ArgumentNullException("folder");encode=serializer;decode=deserializer;}
        public string PathFor(int slot)
        {if(slot<1||slot>Slots)throw new ArgumentOutOfRangeException("slot");return Path.Combine(directory,"rektor-slot-"+slot+".save");}
        public bool HasData(int slot){string path=PathFor(slot);return File.Exists(path)||File.Exists(path+".bak");}
        private static string Checksum(string text)
        {using(SHA256 sha=SHA256.Create()){byte[] b=sha.ComputeHash(Encoding.UTF8.GetBytes(text));return BitConverter.ToString(b).Replace("-","").ToLowerInvariant();}}
        private SchoolState Read(string path)
        {
            var info=new FileInfo(path);if(info.Length>MaxBytes)throw new InvalidDataException("Oversized save");string file=File.ReadAllText(path,Encoding.UTF8);int a=file.IndexOf('\n'),b=a<0?-1:file.IndexOf('\n',a+1);
            if(a<0||b<0||file.Substring(0,a)!="REKTOR 1")throw new InvalidDataException("Invalid save header");string payload=file.Substring(b+1);
            if(file.Substring(a+1,b-a-1)!=Checksum(payload))throw new InvalidDataException("Save checksum mismatch");SchoolState state=decode(payload);
            if(!SchoolSimulation.MigrateSave(state))throw new InvalidDataException("Invalid school state");return state;
        }
        public SchoolState Load(int slot,out string error)
        {
            error=null;string path=PathFor(slot);foreach(string candidate in new[]{path,path+".bak"})
            {if(!File.Exists(candidate))continue;try{SchoolState s=Read(candidate);if(candidate!=path)error="backup";return s;}catch(Exception e){error=e.Message;}}
            return null;
        }
        public bool Write(int slot,SchoolState state,out string error)
        {
            error=null;try
            {
                if(!SchoolSimulation.ValidateSave(state))throw new InvalidDataException("Invalid school state");string path=PathFor(slot);Directory.CreateDirectory(directory);string payload=encode(state);byte[] bytes=Encoding.UTF8.GetBytes("REKTOR 1\n"+Checksum(payload)+"\n"+payload);if(bytes.Length>MaxBytes)throw new InvalidDataException("Oversized save");
                string temp=path+".tmp";using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)){stream.Write(bytes,0,bytes.Length);stream.Flush(true);}
                bool primaryValid=false;if(File.Exists(path)){try{Read(path);primaryValid=true;}catch{}}
                if(File.Exists(path))
                {
                    try{File.Replace(temp,path,primaryValid?path+".bak":null,true);}
                    catch(PlatformNotSupportedException){if(primaryValid)File.Copy(path,path+".bak",true);File.Delete(path);File.Move(temp,path);}
                }
                else File.Move(temp,path);
                return true;
            }
            catch(Exception e){error=e.Message;return false;}
        }
        public void Delete(int slot)
        {string path=PathFor(slot);foreach(string f in new[]{path,path+".bak",path+".tmp"})if(File.Exists(f))File.Delete(f);}
    }
}
