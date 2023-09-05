using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public interface IGroupRepository
    {
        //Login Page
        bool IsUserAnOwner(int userId);
        int GetOwnerGroupId(int userId);
        bool IsUserActiveMemberOfGroup(int userId);
        int GetUserGroupId(int userId);
    }
}
