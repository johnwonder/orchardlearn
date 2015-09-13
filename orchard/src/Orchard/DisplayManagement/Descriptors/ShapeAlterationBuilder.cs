using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DisplayManagement.Descriptors {
    public class ShapeAlterationBuilder {
        Feature _feature;
        readonly string _shapeType;
        readonly string _bindingName;
        readonly IList<Action<ShapeDescriptor>> _configurations = new List<Action<ShapeDescriptor>>();

        public ShapeAlterationBuilder(Feature feature, string shapeType) {
            _feature = feature;
            _bindingName = shapeType;
            var delimiterIndex = shapeType.IndexOf("__");

            if (delimiterIndex < 0) {
                _shapeType = shapeType;
            }
            else {
                _shapeType = shapeType.Substring(0, delimiterIndex);
            }
        }

        public ShapeAlterationBuilder From(Feature feature) {
            _feature = feature;
            return this;
        }

        public ShapeAlterationBuilder Configure(Action<ShapeDescriptor> action) {
            _configurations.Add(action);
            return this;
        }

        public ShapeAlterationBuilder BoundAs(string bindingSource, Func<ShapeDescriptor, Func<DisplayContext, IHtmlString>> binder) {
            // schedule the configuration
            //安排配置,放入_configurations集合中
            return Configure(descriptor => {

                Func<DisplayContext, IHtmlString> target = null;

                var binding = new ShapeBinding {
                    ShapeDescriptor = descriptor,//内部的ShapeDescriptor引用了descriptor
                    BindingName = _bindingName,//ShapeType
                    BindingSource = bindingSource,
                    Binding = displayContext => {

                        // when used, first realize the actual target once
                        if (target == null)
                            target = binder(descriptor);
                        //实现Func<ShapeDescriptor, Func<DisplayContext, IHtmlString>> 
                        //返回Func<DisplayContext,IHtmlString>

                        // and execute the re
                        //执行Func<DisplayContext, IHtmlString>
                        //返回IHtmlString
                        return target(displayContext);
                    }
                };

                // ShapeDescriptor.Bindings is a case insensitive dictionary
                //Bings是个区分大小写的字典
                //最后其实是放入Descriptor中
                descriptor.Bindings[_bindingName] = binding;

            });
        }

        public ShapeAlterationBuilder OnCreating(Action<ShapeCreatingContext> action) {
            return Configure(descriptor => {
                var existing = descriptor.Creating ?? Enumerable.Empty<Action<ShapeCreatingContext>>();
                descriptor.Creating = existing.Concat(new[] { action });
            });
        }

        public ShapeAlterationBuilder OnCreated(Action<ShapeCreatedContext> action) {
            return Configure(descriptor => {
                var existing = descriptor.Created ?? Enumerable.Empty<Action<ShapeCreatedContext>>();
                descriptor.Created = existing.Concat(new[] { action });
            });
        }

        public ShapeAlterationBuilder OnDisplaying(Action<ShapeDisplayingContext> action) {
            return Configure(descriptor => {
                var existing = descriptor.Displaying ?? Enumerable.Empty<Action<ShapeDisplayingContext>>();
                descriptor.Displaying = existing.Concat(new[] { action });
            });
        }

        /// <summary>
        /// 添加 Displayed集合 的Action
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public ShapeAlterationBuilder OnDisplayed(Action<ShapeDisplayedContext> action) {
            return Configure(descriptor => {
                var existing = descriptor.Displayed ?? Enumerable.Empty<Action<ShapeDisplayedContext>>();
                descriptor.Displayed = existing.Concat(new[] { action });
            });
        }

        public ShapeAlterationBuilder Placement(Func<ShapePlacementContext, PlacementInfo> action) {
            return Configure(descriptor => {
                var next = descriptor.Placement;
                descriptor.Placement = ctx => action(ctx) ?? next(ctx);
            });
        }

        public ShapeAlterationBuilder Placement(Func<ShapePlacementContext, bool> predicate, PlacementInfo location) {
            return Configure(descriptor => {
                var next = descriptor.Placement;
                descriptor.Placement = ctx => predicate(ctx) ? location : next(ctx);
            });
        }
        
        /// <summary>
        /// 返回ShapeAlteration实例
        /// _configurations IList<Action<ShapeDescriptor>>集合
        /// </summary>
        /// <returns></returns>
        public ShapeAlteration Build() {
            return new ShapeAlteration(_shapeType, _feature, _configurations.ToArray());
        }
    }

    public class ShapePlacementContext {
        public string ContentType { get; set; }
        public string Stereotype { get; set; }
        public string DisplayType { get; set; }
        public string Differentiator { get; set; }
        public string Path { get; set; }
        public string Source { get; set; }
    }
}
