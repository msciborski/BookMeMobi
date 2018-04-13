using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace BookMeMobi2.Entities
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Cover> Covers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(entity => entity.Property
                                (p => p.Id).HasMaxLength(128));
            builder.Entity<User>(entity => entity.Property
                        (p => p.NormalizedEmail).HasMaxLength(128));
            builder.Entity<User>(entity => entity.Property
                    (p => p.NormalizedUserName).HasMaxLength(128));

            builder.Entity<IdentityRole>(entity => entity.Property
                    (p => p.Id).HasMaxLength(128));
            builder.Entity<IdentityRole>(entity => entity.Property
                    (p => p.NormalizedName).HasMaxLength(128));

            builder.Entity<IdentityUserToken<string>>(entity => entity.Property
                    (p => p.LoginProvider).HasMaxLength(128));
            builder.Entity<IdentityUserToken<string>>(entity => entity.Property
                    (p => p.UserId).HasMaxLength(128));
            builder.Entity<IdentityUserToken<string>>(entity => entity.Property
                    (p => p.Name).HasMaxLength(128));

            builder.Entity<IdentityUserRole<string>>(entity => entity.Property
                    (p => p.UserId).HasMaxLength(128));
            builder.Entity<IdentityUserRole<string>>(entity => entity.Property
                    (p => p.RoleId).HasMaxLength(128));


            builder.Entity<IdentityUserLogin<string>>(entity => entity.Property
                    (p => p.LoginProvider).HasMaxLength(128));
            builder.Entity<IdentityUserLogin<string>>(entity => entity.Property
                    (p => p.ProviderKey).HasMaxLength(128));
            builder.Entity<IdentityUserLogin<string>>(entity => entity.Property
                    (p => p.UserId).HasMaxLength(128));

            builder.Entity<IdentityUserClaim<string>>(entity => entity.Property
                    (p => p.Id).HasMaxLength(128));
            builder.Entity<IdentityUserClaim<string>>(entity => entity.Property
                    (p => p.UserId).HasMaxLength(128));

            builder.Entity<IdentityRoleClaim<string>>(entity => entity.Property
                    (p => p.Id).HasMaxLength(128));
            builder.Entity<IdentityRoleClaim<string>>(entity => entity.Property
                    (p => p.RoleId).HasMaxLength(128));
        }

        public async Task SeedDatabaseWithUsersAndBooks()
        {
            var users = await CreateUsers();
            var books = CreateBooks(users);

            //await this.Users.AddRangeAsync(users);
            await this.Books.AddRangeAsync(books);

            await this.SaveChangesAsync();
        }

        private List<Book> CreateBooks(List<User> users)
        {
            var books = new List<Book>()
            {
                new Book()
                {
                    Id = 1,
                    Title = "Testowy Tytul 1",
                    Author = "Testowy Autor 1",
                    FileName = "testowy1.mobi",
                    PublishingDate = DateTime.Now.AddDays(10).ToUniversalTime(),
                    StoragePath = "testowa/sciezka/testowy1.mobi",
                    UploadDate = DateTime.Now.ToUniversalTime(),
                    Size = 0.543,
                    Format = "mobi",
                    User = users[0]
                },
                new Book()
                {
                    Id = 2,
                    Title = "Testowy Tytul 2",
                    Author = "Testowy Autor 2",
                    FileName = "testowy2.mobi",
                    PublishingDate = DateTime.Now.AddDays(10).ToUniversalTime(),
                    StoragePath = "testowa/sciezka/testowy2.mobi",
                    UploadDate = DateTime.Now.ToUniversalTime(),
                    Size = 0.555,
                    Format = "mobi",
                    User = users[0]
                },
                new Book()
                {
                    Id = 3,
                    Title = "Testowy Tytul 3",
                    Author = "Testowy Autor 3",
                    FileName = "testowy3.mobi",
                    PublishingDate = DateTime.Now.AddDays(10).ToUniversalTime(),
                    StoragePath = "testowa/sciezka/testowy3.mobi",
                    UploadDate = DateTime.Now.ToUniversalTime(),
                    Size = 0.555,
                    Format = "mobi",
                    User = users[0],
                    IsDeleted = true,
                    DeleteDate = DateTime.Now.AddDays(15).ToUniversalTime()
                },
                new Book()
                {
                    Id = 4,
                    Title = "Testowy Tytul 4",
                    Author = "Testowy Autor 4",
                    FileName = "testowy4.mobi",
                    PublishingDate = DateTime.Now.AddDays(10).ToUniversalTime(),
                    StoragePath = "testowa/sciezka/testowy4.mobi",
                    UploadDate = DateTime.Now.ToUniversalTime(),
                    Size = 0.555,
                    Format = "mobi",
                    User = users[1]
                },
                new Book()
                {
                    Id = 5,
                    Title = "Testowy Tytul 5",
                    Author = "Testowy Autor 5",
                    FileName = "testowy5.mobi",
                    PublishingDate = DateTime.Now.AddDays(10).ToUniversalTime(),
                    StoragePath = "testowa/sciezka/testowy5.mobi",
                    UploadDate = DateTime.Now.ToUniversalTime(),
                    Size = 0.555,
                    Format = "mobi",
                    User = users[2]
                }
            };
            return books;
        }
        private async Task<List<User>> CreateUsers()
        {
            var userList = new List<User>
            {
                new User()
                {
                    Id = "ID1",
                    FirstName = "Testowy1",
                    LastName = "TestoweNaziwsko1",
                    UserName = "TestowyUserName1",
                    NormalizedUserName = "TESTOWYUSERNAME1",
                    Email = "testowy@email.com",
                    NormalizedEmail = "TESTOWY@EMAIL.COM",
                    SecurityStamp = Guid.NewGuid().ToString("D")
                },
                new User()
                {
                    Id = "ID2",
                    FirstName = "Testowy2",
                    LastName = "TestoweNaziwsko2",
                    UserName = "TestowyUserName2",
                    NormalizedUserName = "TESTOWYUSERNAME2",
                    Email = "testowy2@email.com",
                    NormalizedEmail = "TESTOWY2@EMAIL.COM",
                    SecurityStamp = Guid.NewGuid().ToString("D")
                },
                new User()
                {
                    Id = "ID3",
                    FirstName = "Testowy3",
                    LastName = "TestoweNaziwsko3",
                    UserName = "TestowyUserName3",
                    NormalizedUserName = "TESTOWYUSERNAME3",
                    Email = "testowy3@email.com",
                    NormalizedEmail = "TESTOWY3@EMAIL.COM",
                    SecurityStamp = Guid.NewGuid().ToString("D")
                }
            };
            userList.ForEach(async u => await CreatePasswordForUser("testowyPassword!123", u));

            return userList;
        }
        private async Task CreatePasswordForUser(string password, User user)
        {
            var passwordHasher = new PasswordHasher<User>();
            var hashedPassword = passwordHasher.HashPassword(user, password);
            user.PasswordHash = hashedPassword;
            
            var userStore = new UserStore<User>(this);
            await userStore.CreateAsync(user);
        }
    }
}
