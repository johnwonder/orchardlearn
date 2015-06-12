using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.Data.Conventions {
    public class RecordTableNameConvention : IClassConvention {
        private readonly Dictionary<Type, RecordBlueprint> _descriptors;

        public RecordTableNameConvention(IEnumerable<RecordBlueprint> descriptors) {
            //转换成Type - RecodrBlueprint的Dictionary
            _descriptors = descriptors.ToDictionary(d => d.Type);

        }

        public void Apply(IClassInstance instance) {
            RecordBlueprint desc;
            if (_descriptors.TryGetValue(instance.EntityType, out desc)) {
                instance.Table(desc.TableName);
                ////定义生成表名的规则，desc.TableName  
            }
        }
    }
}