using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace WebApp.Models
{
    // ApplicationUser クラスにプロパティを追加することでユーザーのプロファイル データを追加できます。詳細については、http://go.microsoft.com/fwlink/?LinkID=317594 を参照してください。
    public class ApplicationUser : IdentityUser
    {
        public string Hometown { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // authenticationType が CookieAuthenticationOptions.AuthenticationType で定義されているものと一致している必要があります
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // ここにカスタム ユーザー クレームを追加します
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

#if AAA
        #region 修正
        // テーブル:AspNetUsersを変更
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var conf1 = modelBuilder.Entity<IdentityUser>().ToTable("CMSMユーザ");
            //conf1.Property(p => p.Id).HasColumnName("ユーザID");
            conf1.Property(p => p.UserName).HasColumnName("ユーザID");
            conf1.Property(p => p.Email).HasColumnName("メールアドレス");
            conf1.Property(p => p.PasswordHash).HasColumnName("パスワード");
            conf1.Ignore(p => p.EmailConfirmed);
            conf1.Ignore(p => p.PhoneNumber);
            conf1.Ignore(p => p.PhoneNumberConfirmed);
            conf1.Ignore(p => p.SecurityStamp);
            conf1.Ignore(p => p.TwoFactorEnabled);
            conf1.Ignore(p => p.SecurityStamp);
            conf1.Ignore(p => p.LockoutEndDateUtc);
            conf1.Ignore(p => p.LockoutEnabled);
            conf1.Ignore(p => p.AccessFailedCount);

            var conf2 = modelBuilder.Entity<ApplicationUser>().ToTable("CMSMユーザ");
            //conf2.Property(p => p.Id).HasColumnName("ユーザーID");
            conf2.Property(p => p.Hometown).HasColumnName("組織CD");

            // 基底クラスのOnModelCreatingメソッド呼び出し
            base.OnModelCreating(modelBuilder);
        }
        #endregion
#endif
    }
}