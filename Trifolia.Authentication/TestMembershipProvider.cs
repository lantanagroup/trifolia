using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace Trifolia.Authentication
{
    public class TestMembershipProvider : MembershipProvider
    {
        private string applicationName = "TEST";

        public override string ApplicationName
        {
            get { return applicationName; }
            set { applicationName = value; }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override bool EnablePasswordReset
        {
            get { return false; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredPasswordLength
        {
            get { throw new NotImplementedException(); }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { throw new NotImplementedException(); }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
#if DEBUG
            Dictionary<string, string> users = new Dictionary<string, string>();
            users.Add("admin", "tr1fol1atest");
            users.Add("igAdmin", "tr1fol1atest");
            users.Add("schemaAdmin", "tr1fol1atest");
            users.Add("templateAuthor", "tr1fol1atest");
            users.Add("user", "tr1fol1atest");
            users.Add("sean.mcilvenna", "tr1fol1atest1");
            users.Add("meenaxi.gosai", "tr1fol1atest2");
            users.Add("keith.boone", "UaUi5hdj");
            users.Add("student1", "student");
            users.Add("student2", "student");
            users.Add("student3", "student");
            users.Add("student4", "student");
            users.Add("student5", "student");
            users.Add("student6", "student");
            users.Add("student7", "student");
            users.Add("student8", "student");
            users.Add("student9", "student");
            users.Add("student10", "student");
            users.Add("student11", "student");
            users.Add("student12", "student");
            users.Add("student13", "student");
            users.Add("student14", "student");
            users.Add("student15", "student");
            users.Add("student16", "student");
            users.Add("student17", "student");
            users.Add("student18", "student");
            users.Add("student19", "student");
            users.Add("student20", "student");

            if (string.IsNullOrEmpty(username))
                return false;

            if (!users.ContainsKey(username))
                return false;

            return users[username] == password;
#endif

            return false;
        }
    }
}
