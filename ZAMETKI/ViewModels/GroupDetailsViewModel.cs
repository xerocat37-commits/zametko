using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ZAMETKI.Api;
using ZAMETKI.Api.Contracts;
using ZAMETKI.Services;

namespace ZAMETKI.ViewModels;

public partial class GroupDetailsViewModel : ObservableObject
{
    private readonly GroupsApi _groups;
    private readonly GroupMembersApi _members;
    private readonly GroupNotesApi _notes;

    public GroupDetailsViewModel(GroupsApi groups, GroupMembersApi members, GroupNotesApi notes)
    {
        _groups = groups;
        _members = members;
        _notes = notes;
    }

    public ObservableCollection<GroupMemberDto> Members { get; } = new();
    public ObservableCollection<NoteDto> Notes { get; } = new();

    [ObservableProperty] private string? groupId;
    [ObservableProperty] private string groupName = string.Empty;
    [ObservableProperty] private bool isBusy;

    public async Task LoadAsync(string id)
    {
        GroupId = id;
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var group = await _groups.GetByIdAsync(id);
            GroupName = group.Name;

            var members = await _members.ListAsync(id);
            Members.Clear();
            foreach (var m in members ?? new List<GroupMemberDto>()) Members.Add(m);

            var notes = await _notes.ListAsync(id);
            Notes.Clear();
            foreach (var n in notes ?? new List<NoteDto>()) Notes.Add(n);
        }
        catch (ApiException ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Сервер недоступен: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> InviteAsync(string studentFullName)
    {
        if (GroupId is null) return false;
        try
        {
            var member = await _members.InviteAsync(GroupId, studentFullName);
            Members.Add(member);
            return true;
        }
        catch (ApiException ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            return false;
        }
    }

    public async Task<bool> CreateNoteAsync(string title, string content)
    {
        if (GroupId is null) return false;
        try
        {
            var note = await _notes.CreateAsync(GroupId, title, content);
            Notes.Insert(0, note);
            return true;
        }
        catch (ApiException ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            return false;
        }
    }
}
