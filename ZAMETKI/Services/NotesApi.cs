using ZAMETKI.Api;
using ZAMETKI.Api.Contracts;

namespace ZAMETKI.Services;

public class NotesApi
{
    private readonly ApiClient _api;

    public NotesApi(ApiClient api) => _api = api;

    public Task<List<NoteDto>> GetMyAsync() =>
        _api.GetAsync<List<NoteDto>>("/api/notes");

    public Task<NoteDto> CreateAsync(string title, string content) =>
        _api.PostAsync<NoteDto>("/api/notes", new CreateNoteRequest(title, content));

    public Task<NoteDto> UpdateAsync(string id, string title, string content) =>
        _api.PutAsync<NoteDto>($"/api/notes/{id}", new UpdateNoteRequest(title, content));

    public Task DeleteAsync(string id) => _api.DeleteAsync($"/api/notes/{id}");

    public Task<List<NoteDto>> GetFeedAsync() =>
        _api.GetAsync<List<NoteDto>>("/api/notes/feed");
}
