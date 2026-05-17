using ZAMETKI.Api;
using ZAMETKI.Api.Contracts;

namespace ZAMETKI.Services;

public class GlobalNotesApi
{
    private readonly ApiClient _api;

    public GlobalNotesApi(ApiClient api) => _api = api;

    public Task<NoteDto> CreateAsync(string title, string content) =>
        _api.PostAsync<NoteDto>("/api/notes/global", new CreateNoteRequest(title, content));
}
