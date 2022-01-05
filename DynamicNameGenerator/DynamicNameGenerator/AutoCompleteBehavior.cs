// ***********************************************************************
// Assembly         : DynamicNameGenerator
// Author           : WPFTextBoxAutoComplete
// Created          : 01-05-2022
//
// Last Modified By : Mario
// Last Modified On : 01-06-2022
// ***********************************************************************
// <copyright file="AutoCompleteBehavior.cs" company="WPFTextBoxAutoComplete">
//     WPFTextBoxAutoComplete
// </copyright>
// <summary></summary>
// ***********************************************************************

// Source: https://github.com/Nimgoble/WPFTextBoxAutoComplete/blob/master/WPFTextBoxAutoComplete/AutoCompleteBehavior.cs
// License: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPFTextBoxAutoComplete
{
    /// <summary>
    /// Class AutoCompleteBehavior.
    /// </summary>
    public static class AutoCompleteBehavior
    {
        #region Fields

        /// <summary>
        /// What string should indicate that we should start giving auto-completion suggestions.  For example: @
        /// If this is null or empty, auto-completion suggestions will begin at the beginning of the textbox's text.
        /// </summary>
        public static readonly DependencyProperty AutoCompleteIndicator =
            DependencyProperty.RegisterAttached
            (
                "AutoCompleteIndicator",
                typeof(String),
                typeof(AutoCompleteBehavior),
                new UIPropertyMetadata(String.Empty)
            );

        /// <summary>
        /// The collection to search for matches from.
        /// </summary>
        public static readonly DependencyProperty AutoCompleteItemsSource =
            DependencyProperty.RegisterAttached
            (
                "AutoCompleteItemsSource",
                typeof(IEnumerable<String>),
                typeof(AutoCompleteBehavior),
                new UIPropertyMetadata(null, OnAutoCompleteItemsSource)
            );

        /// <summary>
        /// Whether or not to ignore case when searching for matches.
        /// </summary>
        public static readonly DependencyProperty AutoCompleteStringComparison =
            DependencyProperty.RegisterAttached
            (
                "AutoCompleteStringComparison",
                typeof(StringComparison),
                typeof(AutoCompleteBehavior),
                new UIPropertyMetadata(StringComparison.Ordinal)
            );

        /// <summary>
        /// The on key down
        /// </summary>
        private static KeyEventHandler onKeyDown = new KeyEventHandler(OnPreviewKeyDown);

        /// <summary>
        /// The on text changed
        /// </summary>
        private static TextChangedEventHandler onTextChanged = new TextChangedEventHandler(OnTextChanged);

        #endregion Fields

        #region Methods

        /// <summary>
        /// Gets the automatic complete indicator.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>String.</returns>
        public static String GetAutoCompleteIndicator(DependencyObject obj)
        {
            return (String)obj.GetValue(AutoCompleteIndicator);
        }

        /// <summary>
        /// Gets the automatic complete items source.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>IEnumerable&lt;String&gt;.</returns>
        public static IEnumerable<String> GetAutoCompleteItemsSource(DependencyObject obj)
        {
            object objRtn = obj.GetValue(AutoCompleteItemsSource);
            if (objRtn is IEnumerable<String>)
                return (objRtn as IEnumerable<String>);

            return null;
        }

        /// <summary>
        /// Gets the automatic complete string comparison.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>StringComparison.</returns>
        public static StringComparison GetAutoCompleteStringComparison(DependencyObject obj)
        {
            return (StringComparison)obj.GetValue(AutoCompleteStringComparison);
        }

        /// <summary>
        /// Sets the automatic complete indicator.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetAutoCompleteIndicator(DependencyObject obj, String value)
        {
            obj.SetValue(AutoCompleteIndicator, value);
        }

        /// <summary>
        /// Sets the automatic complete items source.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetAutoCompleteItemsSource(DependencyObject obj, IEnumerable<String> value)
        {
            obj.SetValue(AutoCompleteItemsSource, value);
        }

        /// <summary>
        /// Sets the automatic complete string comparison.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetAutoCompleteStringComparison(DependencyObject obj, StringComparison value)
        {
            obj.SetValue(AutoCompleteStringComparison, value);
        }

        /// <summary>
        /// Handles the <see cref="E:AutoCompleteItemsSource" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void OnAutoCompleteItemsSource(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (sender == null)
                return;

            //If we're being removed, remove the callbacks
            //Remove our old handler, regardless of if we have a new one.
            tb.TextChanged -= onTextChanged;
            tb.PreviewKeyDown -= onKeyDown;
            if (e.NewValue != null)
            {
                //New source.  Add the callbacks
                tb.TextChanged += onTextChanged;
                tb.PreviewKeyDown += onKeyDown;
            }
        }

        /// <summary>
        /// Used for moving the caret to the end of the suggested auto-completion text.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            TextBox tb = e.OriginalSource as TextBox;
            if (tb == null)
                return;

            //If we pressed enter and if the selected text goes all the way to the end, move our caret position to the end
            if (tb.SelectionLength > 0 && (tb.SelectionStart + tb.SelectionLength == tb.Text.Length))
            {
                tb.SelectionStart = tb.CaretIndex = tb.Text.Length;
                tb.SelectionLength = 0;
            }
        }

        /// <summary>
        /// Search for auto-completion suggestions.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if
            (
                (from change in e.Changes where change.RemovedLength > 0 select change).Any() &&
                (from change in e.Changes where change.AddedLength > 0 select change).Any() == false
            )
                return;

            TextBox tb = e.OriginalSource as TextBox;
            if (sender == null)
                return;

            IEnumerable<String> values = GetAutoCompleteItemsSource(tb);
            //No reason to search if we don't have any values.
            if (values == null)
                return;

            //No reason to search if there's nothing there.
            if (String.IsNullOrEmpty(tb.Text))
                return;

            string indicator = GetAutoCompleteIndicator(tb);
            int startIndex = 0; //Start from the beginning of the line.
            string matchingString = tb.Text;
            //If we have a trigger string, make sure that it has been typed before
            //giving auto-completion suggestions.
            if (!String.IsNullOrEmpty(indicator))
            {
                startIndex = tb.Text.LastIndexOf(indicator);
                //If we haven't typed the trigger string, then don't do anything.
                if (startIndex == -1)
                    return;

                startIndex += indicator.Length;
                matchingString = tb.Text.Substring(startIndex, (tb.Text.Length - startIndex));
            }

            //If we don't have anything after the trigger string, return.
            if (String.IsNullOrEmpty(matchingString))
                return;

            Int32 textLength = matchingString.Length;

            StringComparison comparer = GetAutoCompleteStringComparison(tb);
            //Do search and changes here.
            String match =
            (
                from
                    value
                in
                (
                    from subvalue
                    in values
                    where subvalue != null && subvalue.Length >= textLength
                    select subvalue
                )
                where value.Substring(0, textLength).Equals(matchingString, comparer)
                select value.Substring(textLength, value.Length - textLength)/*Only select the last part of the suggestion*/
            ).FirstOrDefault();

            //Nothing.  Leave 'em alone
            if (String.IsNullOrEmpty(match))
                return;

            int matchStart = (startIndex + matchingString.Length);
            tb.TextChanged -= onTextChanged;
            tb.Text += match;
            tb.CaretIndex = matchStart;
            tb.SelectionStart = matchStart;
            tb.SelectionLength = (tb.Text.Length - startIndex);
            tb.TextChanged += onTextChanged;
        }

        #endregion Methods
    }
}