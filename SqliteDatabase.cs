using System;
using System.Net;
using System.Collections.Generic;

using Community.CsharpSqlite;
using SQLite;

namespace GoogleVoice
{
    public static class SqliteDatabase
    {
        static SQLiteConnection mConnection = new SQLiteConnection("application.db");

        public static SQLiteConnection Connection
        {
            get
            {
                return mConnection;
            }
        }

        public static Sqlite3.sqlite3 Instance
        {
            get
            {
                return mConnection.Handle;
            }
        }
    }
}
