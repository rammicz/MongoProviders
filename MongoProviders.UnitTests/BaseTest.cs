﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Web.Configuration;
using System.Configuration;
using System.Web.Security;
using MongoDB.Bson;
using System.Web.Profile;
using System.Collections.Specialized;

namespace MongoProviders.UnitTests
{
    public class BaseTest
    {

        protected IMongoDatabase _db;
        protected string _connStrName;
        protected string _applicationName;
        protected IMongoCollection<User> _userCollection;
        protected IMongoCollection<BsonDocument> _roleCollection;
        protected const string _appName2 = "/myapp";
        protected const string Membership_missing_application_name = "App.Config system.web/membership section is missing an applicationName attribute.  This is required so all tests point to the same data collection.";
        protected const string Roles_missing_application_name = "App.Config system.web/roleManager section is missing an applicationName attribute.  This is required so all tests point to the same data collection.";
        protected const string Application_names_must_match = "App.Config membership and roleManager sections must use identical applicationName attribute values.  This is required so that all tests point to the same data collection.";

        public virtual void Setup()
        {
            EnsureApplicationNameSpecified(out _applicationName);

            try
            {
                _connStrName = ConfigurationManager.ConnectionStrings[0].Name;
            }
            catch (Exception)
            {
                throw new Exception("Missing connection string in App.config");
            }

            var mem = (MongoProviders.MembershipProvider)Membership.Provider;
            _db = mem.Database;
            _userCollection = mem.Collection;

            var role = (MongoProviders.RoleProvider)Roles.Provider;
            _roleCollection = role.RoleCollection;

            DropCollections();
        }

        protected void DropCollections()
        {
            var roleProvider = new RoleProvider();

            // roles
            var colName = Helper.GenerateCollectionName(_applicationName, RoleProvider.DEFAULT_ROLE_COLLECTION_SUFFIX);
            _db.DropCollection(colName);

            colName = Helper.GenerateCollectionName(_appName2, RoleProvider.DEFAULT_ROLE_COLLECTION_SUFFIX);
            _db.DropCollection(colName);

            // users
            colName = Helper.GenerateCollectionName(_applicationName, MembershipProvider.DEFAULT_USER_COLLECTION_SUFFIX);
            _db.DropCollection(colName);

            colName = Helper.GenerateCollectionName(_appName2, MembershipProvider.DEFAULT_USER_COLLECTION_SUFFIX);
            _db.DropCollection(colName);

        }

        protected void EnsureApplicationNameSpecified(out string appName)
        {
            string mem = null;
            string roles = null;

            // Membership
            try
            {
                MembershipSection section = (MembershipSection)WebConfigurationManager.GetSection("system.web/membership");
                string defaultProvider = section.DefaultProvider;
                ProviderSettings providerSettings = section.Providers[defaultProvider];
                mem = providerSettings.Parameters["applicationName"];
            }
            catch (Exception)
            {
                throw new Exception(Membership_missing_application_name);
            }
            if (null == mem)
                throw new Exception(Membership_missing_application_name);


            // Roles 
            try
            {
                RoleManagerSection section = (RoleManagerSection)WebConfigurationManager.GetSection("system.web/roleManager");
                string defaultProvider = section.DefaultProvider;
                ProviderSettings providerSettings = section.Providers[defaultProvider];
                roles = providerSettings.Parameters["applicationName"];
            }
            catch (Exception)
            {
                throw new Exception(Roles_missing_application_name);
            }
            if (null == roles)
                throw new Exception(Roles_missing_application_name);


            // match?
            if (roles != mem)
                throw new Exception(Application_names_must_match);

            appName = mem;
        }

    }
}
