using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public interface IGroupRepository
    {
        //Login Page
        bool IsUserAnOwner(LoginUserDTO userData);
        int GetOwnerGroupId(LoginUserDTO userData);
        bool IsUserActiveMemberOfGroup(LoginUserDTO userData);
        int GetUserGroupId(LoginUserDTO userData);
    }
}
