using ZAMETKI.Models;
using ZAMETKI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ZAMETKI
{
    public partial class MainPage : ContentPage
    {
        private readonly NoteService _noteService;
        private ObservableCollection<Note> _notes;

        public MainPage()
        {
            InitializeComponent();
            _noteService = new NoteService();
            _notes = new ObservableCollection<Note>();
            NotesCollectionView.ItemsSource = _notes;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadNotesAsync();
        }

        private async Task LoadNotesAsync()
        {
            try
            {
                var notes = await _noteService.GetNotesAsync();
                _notes.Clear();
                foreach (var note in notes)
                {
                    _notes.Add(note);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить заметки: {ex.Message}", "OK");
            }
        }

        private async void AddNoteButton_Clicked(object sender, EventArgs e)
        {
            await ShowAddNoteDialog();
        }

        private async Task ShowAddNoteDialog()
        {
            string title = await DisplayPromptAsync(
                "Новая заметка",
                "Введите заголовок:",
                placeholder: "Заголовок",
                maxLength: 100,
                keyboard: Keyboard.Text);

            if (string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            string content = await DisplayPromptAsync(
                "Новая заметка",
                "Введите текст заметки:",
                placeholder: "Текст заметки...",
                maxLength: 5000,
                keyboard: Keyboard.Text);

            if (content == null)
            {
                return;
            }

            try
            {
                var newNote = new Note
                {
                    Title = title,
                    Content = content ?? string.Empty
                };

                await _noteService.AddNoteAsync(newNote);
                await LoadNotesAsync();
                await DisplayAlert("Успех", "Заметка добавлена!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось добавить заметку: {ex.Message}", "OK");
            }
        }

        private async void NotesCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Note selectedNote)
            {
                await ShowEditNoteDialog(selectedNote);
                NotesCollectionView.SelectedItem = null;
            }
        }

        private async Task ShowEditNoteDialog(Note note)
        {
            var action = await DisplayActionSheet(
                $"{note.Title}",
                "Отмена",
                "Удалить",
                "Редактировать заголовок",
                "Редактировать содержимое");

            switch (action)
            {
                case "Редактировать заголовок":
                    await EditNoteTitleAsync(note);
                    break;
                case "Редактировать содержимое":
                    await EditNoteContentAsync(note);
                    break;
                case "Удалить":
                    await DeleteNoteAsync(note);
                    break;
            }
        }

        private async Task EditNoteTitleAsync(Note note)
        {
            string newTitle = await DisplayPromptAsync(
                "Редактировать заголовок",
                "Введите новый заголовок:",
                initialValue: note.Title,
                maxLength: 100,
                keyboard: Keyboard.Text);

            if (!string.IsNullOrWhiteSpace(newTitle) && newTitle != note.Title)
            {
                note.Title = newTitle;
                await _noteService.UpdateNoteAsync(note);
                await LoadNotesAsync();
            }
        }

        private async Task EditNoteContentAsync(Note note)
        {
            string newContent = await DisplayPromptAsync(
                "Редактировать содержимое",
                "Введите новый текст:",
                initialValue: note.Content,
                maxLength: 5000,
                keyboard: Keyboard.Text);

            if (newContent != null && newContent != note.Content)
            {
                note.Content = newContent;
                await _noteService.UpdateNoteAsync(note);
                await LoadNotesAsync();
            }
        }

        private async Task DeleteNoteAsync(Note note)
        {
            bool confirm = await DisplayAlert(
                "Подтверждение",
                $"Удалить заметку '{note.Title}'?",
                "Удалить",
                "Отмена");

            if (confirm)
            {
                await _noteService.DeleteNoteAsync(note);
                await LoadNotesAsync();
            }
        }

        private async void DeleteSwipeItem_Invoked(object sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.BindingContext is Note note)
            {
                await DeleteNoteAsync(note);
            }
        }

        private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(e.NewTextValue))
                {
                    await LoadNotesAsync();
                }
                else
                {
                    var searchResults = await _noteService.SearchNotesAsync(e.NewTextValue);
                    _notes.Clear();
                    foreach (var note in searchResults)
                    {
                        _notes.Add(note);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка поиска: {ex.Message}", "OK");
            }
        }
    }
}
