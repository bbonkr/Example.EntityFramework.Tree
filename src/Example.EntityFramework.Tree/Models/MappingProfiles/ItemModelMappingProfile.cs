using System;
using AutoMapper;
using Example.EntityFramework.Tree.Entities;

namespace Example.EntityFramework.Tree.Models.MappingProfiles;

public class ItemModelMappingProfile : Profile
{
    public ItemModelMappingProfile()
    {
        CreateMap<Item, ItemModel>()
            //.ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children.ToList()))
            ;

        CreateMap<AddItemModel, Item>()
            .ForMember(x => x.Parent, opt => opt.Ignore())
            .ForMember(x => x.Children, opt => opt.Ignore());
    }
}

