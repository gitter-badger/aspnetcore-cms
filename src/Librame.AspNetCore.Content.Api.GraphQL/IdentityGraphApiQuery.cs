﻿#region License

/* **************************************************************************************
 * Copyright (c) Librame Pong All rights reserved.
 * 
 * https://github.com/librame
 * 
 * You must not remove this notice, or any other, from this software.
 * **************************************************************************************/

#endregion

using GraphQL.Types;
using Microsoft.AspNetCore.Content;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Librame.AspNetCore.Content.Api
{
    using AspNetCore.Api;
    using AspNetCore.Content.Api.StoreTypes;
    using Extensions;
    using Extensions.Data.Collections;
    using Extensions.Data.Stores;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class ContentGraphApiQuery<TRole, TUser> : ObjectGraphType, IGraphApiQuery
        where TRole : class, ICreatedTimeTicks
        where TUser : class, ICreatedTimeTicks
    {
        private readonly RoleManager<TRole> _roleManager;
        private readonly UserManager<TUser> _userManager;


        public ContentGraphApiQuery(RoleManager<TRole> roleManager,
            UserManager<TUser> userManager)
        {
            _roleManager = roleManager.NotNull(nameof(roleManager));
            _userManager = userManager.NotNull(nameof(userManager));

            Name = nameof(ISchema.Query);

            AddRoleTypeFields();
            AddUserTypeFields();
        }


        private void AddRoleTypeFields()
        {
            if (ContentApiSettings.Preference.SupportsQueryRoles)
            {
                // roles(index:1,size:10)
                Field<ContentCategoryType<TRole>>
                (
                    name: "roles",
                    arguments: new QueryArguments(
                        new QueryArgument<IntGraphType> { Name = "index" },
                        new QueryArgument<IntGraphType> { Name = "size" }
                    ),
                    resolve: context =>
                    {
                        var index = context.GetArgument<int?>("index");
                        var size = context.GetArgument<int?>("size");

                        if (index.HasValue && size.HasValue)
                        {
                            return _roleManager.Roles.AsPagingByIndex(ordered =>
                            {
                                return ordered.OrderBy(k => k.CreatedTimeTicks);
                            },
                            index.Value, size.Value);
                        }

                        return _roleManager.Roles.ToList();
                    }
                );
            }
            
            // role(name:"")
            Field<ContentCategoryType<TRole>>
            (
                name: "role",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }
                ),
                resolve: context =>
                {
                    var name = context.GetArgument<string>("name");
                    return _roleManager.FindByNameAsync(name).ConfigureAndResult();
                }
            );
        }

        private void AddUserTypeFields()
        {
            if (ContentApiSettings.Preference.SupportsQueryUsers)
            {
                // users(index:1,size:10)
                Field<ContentUserType<TUser>>
                (
                    name: "users",
                    arguments: new QueryArguments(
                        new QueryArgument<IntGraphType> { Name = "index" },
                        new QueryArgument<IntGraphType> { Name = "size" }
                    ),
                    resolve: context =>
                    {
                        var index = context.GetArgument<int?>("index");
                        var size = context.GetArgument<int?>("size");

                        if (index.HasValue && size.HasValue)
                        {
                            return _userManager.Users.AsPagingByIndex(ordered =>
                            {
                                return ordered.OrderBy(k => k.CreatedTimeTicks);
                            },
                            index.Value, size.Value);
                        }

                        return _userManager.Users.ToList();
                    }
                );
            }

            // user(name:"")
            Field<ContentUserType<TUser>>
            (
                name: "user",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }
                ),
                resolve: context =>
                {
                    var name = context.GetArgument<string>("name");
                    return _userManager.FindByNameAsync(name).ConfigureAndResult();
                }
            );
        }

    }
}
