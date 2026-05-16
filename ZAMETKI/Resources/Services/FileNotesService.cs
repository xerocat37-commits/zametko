using SQLite;
using ZAMETKI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ZAMETKI.Services
{
    public class NoteService
    {
        private readonly SQLiteAsyncConnection _database;

        public NoteService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "notes.db3");
            _database = new SQLiteAsyncConnection(dbPath);

            // Создаем таблицу при инициализации
            _database.CreateTableAsync<Note>().Wait();
        }

        // Получить все заметки
        public Task<List<Note>> GetNotesAsync()
        {
            return _database.Table<Note>()
                           .OrderByDescending(n => n.ModifiedDate)
                           .ToListAsync();
        }

        // Получить заметку по ID
        public Task<Note> GetNoteByIdAsync(int id)
        {
            return _database.Table<Note>()
                           .Where(n => n.Id == id)
                           .FirstOrDefaultAsync();
        }

        // Добавить заметку
        public Task<int> AddNoteAsync(Note note)
        {
            if (note == null)
                throw new ArgumentNullException(nameof(note));

            note.CreatedDate = DateTime.Now;

            note.ModifiedDate = DateTime.Now;
            return _database.InsertAsync(note);
        }

        // Обновить заметку
        public Task<int> UpdateNoteAsync(Note note)
        {
            if (note == null)
                throw new ArgumentNullException(nameof(note));

            note.ModifiedDate = DateTime.Now;
            return _database.UpdateAsync(note);
        }

        // Удалить заметку
        public Task<int> DeleteNoteAsync(Note note)
        {
            if (note == null)
                throw new ArgumentNullException(nameof(note));

            return _database.DeleteAsync(note);
        }

        // Поиск заметок
        public Task<List<Note>> SearchNotesAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return GetNotesAsync();

            return _database.Table<Note>()
                           .Where(n => n.Title.Contains(searchText) ||
                                      n.Content.Contains(searchText))
                           .OrderByDescending(n => n.ModifiedDate)
                           .ToListAsync();
        }
    }
}