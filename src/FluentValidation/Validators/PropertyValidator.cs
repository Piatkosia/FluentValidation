#region License
// Copyright 2008-2009 Jeremy Skinner (http://www.jeremyskinner.co.uk)
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://www.codeplex.com/FluentValidation
#endregion

namespace FluentValidation.Validators {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using Attributes;
	using Internal;
	using Results;

	public abstract class PropertyValidator : IPropertyValidator {
		private readonly List<Func<object, object>> customFormatArgs = new List<Func<object, object>>();
		private Func<string> resourceAccessor;
		
		public bool SupportsStandaloneValidation { get; set; }

		public string ErrorMessageTemplate {
			get { return resourceAccessor(); }
			set { resourceAccessor = () => value; }
		}

		public ICollection<Func<object, object>> CustomMessageFormatArguments {
			get { return customFormatArgs; }
		}

		protected PropertyValidator(string errorMessageResourceName, Type errorMessageResourceType) {
			resourceAccessor = ResourceHelper.BuildResourceAccessor(errorMessageResourceName, errorMessageResourceType);
		}

		protected PropertyValidator(string errorMessage) {
			resourceAccessor = () => errorMessage;
		}

		protected PropertyValidator(Expression<Func<string>> errorMessageResourceSelector) {
			resourceAccessor = ResourceHelper.BuildResourceAccessor(errorMessageResourceSelector);
		}

		public virtual PropertyValidatorResult Validate(PropertyValidatorContext context) {
			context.MessageFormatter.AppendPropertyName(context.PropertyDescription);

			if (!IsValid(context)) {
				return CreateValidationError(context);
			}

			return PropertyValidatorResult.Success();
		}

		protected abstract bool IsValid(PropertyValidatorContext context);

		/// <summary>
		/// Creates an error validation result for this validator.
		/// </summary>
		/// <param name="context">The validator context</param>
		/// <returns>Returns an error validation result.</returns>
		protected virtual PropertyValidatorResult CreateValidationError(PropertyValidatorContext context) {
			//Continue to support ValidationMessageAttribute for backwards compatibility.
			string error = ErrorMessageTemplate;

			context.MessageFormatter.AppendAdditionalArguments(
				customFormatArgs.Select(func => func(context.Instance)).ToArray()
			);

			return PropertyValidatorResult.Failure(context.MessageFormatter.BuildMessage(error));
		}
	}
}