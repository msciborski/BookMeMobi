using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Models;
using BookMeMobi2.Models.Book;
using BookMeMobi2.Models.User;

namespace BookMeMobi2.Helpers.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserRegisterDto>();
            CreateMap<UserRegisterDto, User>();
            CreateMap<User, UserLoginDto>();
            CreateMap<UserLoginDto, User>();
            CreateMap<User, UserDto>();
            CreateMap<PagedList<User>, PagedList<UserDto>>();

            //Create mapping for many to many relationship
            //between Tags and Books
            CreateMap<BookTag,TagDto>()
              .ForMember(t => t.Id, opt => opt.MapFrom(bt => bt.TagId))
              .ForMember(t => t.Name, opt => opt.MapFrom(bt => bt.Tag.TagName));

            CreateMap<Book, BookDeleteDto>();
            CreateMap<BookDeleteDto, Book>();
            CreateMap<BookDto, Book>();
            CreateMap<Book, BookDto>();
            CreateMap<PagedList<Book>, PagedList<BookDto>>();
            CreateMap<PagedList<BookDto>, PagedList<Book>>();
        }
    }
}
