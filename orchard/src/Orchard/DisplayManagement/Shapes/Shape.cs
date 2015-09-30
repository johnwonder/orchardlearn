using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Mvc;
using ClaySharp;
using ClaySharp.Implementation;

namespace Orchard.DisplayManagement.Shapes {
    /// <summary>
    /// 形状是一个动态对象，利用形状模板，可以使数据以你想要的方式呈现给用户。
    /// 形状模板是一段呈现形状的标记。
    /// 形状的例子有：菜单 菜单项 内容项 文档和消息等。
    /// 一个形状是一个数据模型对象。它继承于Orchard.DisplayManagement.Shapes.Shape类。
    /// 形状类是没有实例化的。相反，形状是在运行时由一个形状工厂创建的
    /// 默认的形状工厂是Orchard.DisplayManagement.Implementation.DefaultShapeFactory.
    /// 形状的信息是包含在自身的ShapeMetadata属性里面。
    /// 这些信息包括形状的类型，显示类型，位置，前缀，包装，替换名称
    /// 子内容和一个是否已执行的标记。
    /// 可以通过shapeName.Metadata.Type来访问形状的元数据。
    /// 数据模型对象
    /// 在形状对象创建以后，形状里面的数据就可通过形状模板中的Help方法来呈现出来。
    /// 一个形状模板是一个段Html标记（部分视图，partial view)是负责显示形状的。
    /// 也可以通过代码来呈现形状。如 定义一个方法并加上Shape特性即可。
    /// 可以在ChoreShapes.cs中找到这种写法。
    /// </summary>
    [DebuggerTypeProxy(typeof(ShapeDebugView))]
    //继承了IEnumerable接口 在
    public class Shape : IShape, IEnumerable {
        private const string DefaultPosition = "5";

        private readonly IList<object> _items = new List<object>();
        private readonly IList<string> _classes = new List<string>();
        private readonly IDictionary<string, string> _attributes = new Dictionary<string, string>();

        public virtual ShapeMetadata Metadata { get; set; }

        public virtual string Id { get; set; }
        public virtual IList<string> Classes { get { return _classes; } }
        public virtual IDictionary<string, string> Attributes { get { return _attributes; } }
        public virtual IEnumerable<dynamic> Items { get { return _items; } }

        public virtual Shape Add(object item, string position = null) {
            // pszmyd: Ignoring null shapes 
            if (item == null) {
                return this;
            }

            try {
                // todo: (sebros) this is a temporary implementation to prevent common known scenarios throwing exceptions. The final solution would need to filter based on the fact that it is a Shape instance
                if ( item is MvcHtmlString ||
                    item is String ) {
                    // need to implement positioned wrapper for non-shape objects
                }
                else if (item is IShape) {
                    ((dynamic) item).Metadata.Position = position;
                }
            }
            catch {
                // need to implement positioned wrapper for non-shape objects
            }

            _items.Add(item); // not messing with position at the moment
            return this;
        }

        public virtual Shape AddRange(IEnumerable<object> items, string position = DefaultPosition) {
            foreach (var item in items)
                Add(item, position);
            return this;
        }

        public virtual IEnumerator GetEnumerator() {
            return _items.GetEnumerator();
        }

        public class ShapeBehavior : ClayBehavior {
            public override object SetIndex(Func<object> proceed, dynamic self, IEnumerable<object> keys, object value) {
                if (keys.Count() == 1) {
                    var name = keys.Single().ToString();
                    if (name.Equals("Id")) {
                        // need to mutate the actual type
                        var s = self as Shape;
                        if (s != null) {
                            s.Id = System.Convert.ToString(value);
                        }
                        return value;
                    }
                    if (name.Equals("Classes")) {
                        var args = Arguments.From(new[] { value }, Enumerable.Empty<string>());
                        MergeClasses(args, self.Classes);
                        return value;
                    }
                    if (name.Equals("Attributes")) {
                        var args = Arguments.From(new[] { value }, Enumerable.Empty<string>());
                        MergeAttributes(args, self.Attributes);
                        return value;
                    }
                    if (name.Equals("Items")) {
                        var args = Arguments.From(new[] { value }, Enumerable.Empty<string>());
                        MergeItems(args, self);
                        return value;
                    }
                }
                return proceed();
            }

            public override object InvokeMember(Func<object> proceed, dynamic self, string name, INamedEnumerable<object> args) {
                if (name.Equals("Id")) {
                    // need to mutate the actual type
                    var s = self as Shape;
                    if (s != null) {
                        s.Id = System.Convert.ToString(args.FirstOrDefault());
                    }
                    return self;
                }
                if (name.Equals("Classes") && !args.Named.Any()) {
                    MergeClasses(args, self.Classes);
                    return self;
                }
                if (name.Equals("Attributes") && args.Positional.Count() <= 1) {
                    MergeAttributes(args, self.Attributes);
                    return self;
                }
                if (name.Equals("Items")) {
                    MergeItems(args, self);
                    return self;
                }
                return proceed();
            }

            private static void MergeAttributes(INamedEnumerable<object> args, IDictionary<string, string> attributes) {
                var arg = args.Positional.SingleOrDefault();
                if (arg != null) {
                    if (arg is IDictionary) {
                        var dictionary = arg as IDictionary;
                        foreach (var key in dictionary.Keys) {
                            attributes[System.Convert.ToString(key)] = System.Convert.ToString(dictionary[key]);
                        }
                    }
                    else {
                        foreach (var prop in arg.GetType().GetProperties()) {
                            attributes[TranslateIdentifier(prop.Name)] = System.Convert.ToString(prop.GetValue(arg, null));
                        }
                    }
                }
                foreach (var named in args.Named) {
                    attributes[named.Key] = System.Convert.ToString(named.Value);
                }
            }

            private static string TranslateIdentifier(string name) {
                // Allows for certain characters in an identifier to represent different
                // characters in an HTML attribute (mimics MVC behavior):
                // data_foo ==> data-foo
                // @keyword ==> keyword
                return name.Replace("_", "-").Replace("@", "");
            }

            private static void MergeClasses(INamedEnumerable<object> args, IList<string> classes) {
                foreach (var arg in args) {
                    // look for string first, because the "string" type is also an IEnumerable of char
                    if (arg is string) {
                        classes.Add(arg as string);
                    }
                    else if (arg is IEnumerable) {
                        foreach (var item in arg as IEnumerable) {
                            classes.Add(System.Convert.ToString(item));
                        }
                    }
                    else {
                        classes.Add(System.Convert.ToString(arg));
                    }
                }
            }

            private static void MergeItems(INamedEnumerable<object> args, dynamic shape) {
                foreach (var arg in args) {
                    // look for string first, because the "string" type is also an IEnumerable of char
                    if (arg is string) {
                        shape.Add(arg as string);
                    }
                    else if (arg is IEnumerable) {
                        foreach (var item in arg as IEnumerable) {
                            shape.Add(item);
                        }
                    }
                    else {
                        shape.Add(System.Convert.ToString(arg));
                    }
                }
            }
        }
    }
}
