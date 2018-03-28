using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Models;

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

            CreateMap<BookDto, Book>();
            CreateMap<Book, BookDto>();
            CreateMap<PagedList<Book>, PagedList<BookDto>>();
            CreateMap<PagedList<BookDto>, PagedList<Book>>();
        }
    }
}
