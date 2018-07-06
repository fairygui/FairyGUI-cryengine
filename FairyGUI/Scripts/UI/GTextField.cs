﻿using System.Collections.Generic;
using System.Text;
using CryEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class GTextField : GObject, ITextColorGear
	{
		protected TextField _textField;
		protected string _text;
		protected bool _ubbEnabled;
		protected bool _updatingSize;
		protected Dictionary<string, string> _templateVars;

		public GTextField()
			: base()
		{
			TextFormat tf = _textField.textFormat;
			tf.font = UIConfig.defaultFont;
			tf.size = 12;
			tf.color = Color.Black;
			tf.lineSpacing = 3;
			tf.letterSpacing = 0;
			_textField.textFormat = tf;

			_text = string.Empty;
			_textField.autoSize = AutoSizeType.Both;
			_textField.wordWrap = false;
		}

		override protected void CreateDisplayObject()
		{
			_textField = new TextField();
			_textField.gOwner = this;
			displayObject = _textField;
		}

		/// <summary>
		/// 
		/// </summary>
		override public string text
		{
			get
			{
				GetTextFieldText();
				return _text;
			}
			set
			{
				if (value == null)
					value = string.Empty;
				_text = value;
				SetTextFieldText();
				UpdateSize();
				UpdateGear(6);
			}
		}

		virtual protected void SetTextFieldText()
		{
			string str = _text;
			if (_templateVars != null)
				str = ParseTemplate(str);

			if (_ubbEnabled)
				_textField.htmlText = UBBParser.inst.Parse(XMLUtils.EncodeString(str));
			else
				_textField.text = str;
		}

		virtual protected void GetTextFieldText()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public Dictionary<string, string> templateVars
		{
			get { return _templateVars; }
			set
			{
				if (_templateVars == null && value == null)
					return;

				_templateVars = value;

				FlushVars();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public GTextField SetVar(string name, string value)
		{
			if (_templateVars == null)
				_templateVars = new Dictionary<string, string>();
			_templateVars[name] = value;

			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		public void FlushVars()
		{
			SetTextFieldText();
			UpdateSize();
		}

		protected string ParseTemplate(string template)
		{
			int pos1 = 0, pos2 = 0;
			int pos3;
			string tag;
			string value;
			StringBuilder buffer = new StringBuilder();

			while ((pos2 = template.IndexOf('{', pos1)) != -1)
			{
				if (pos2 > 0 && template[pos2 - 1] == '\\')
				{
					buffer.Append(template, pos1, pos2 - pos1 - 1);
					buffer.Append('{');
					pos1 = pos2 + 1;
					continue;
				}

				buffer.Append(template, pos1, pos2 - pos1);
				pos1 = pos2;
				pos2 = template.IndexOf('}', pos1);
				if (pos2 == -1)
					break;

				if (pos2 == pos1 + 1)
				{
					buffer.Append(template, pos1, 2);
					pos1 = pos2 + 1;
					continue;
				}

				tag = template.Substring(pos1 + 1, pos2 - pos1 - 1);
				pos3 = tag.IndexOf('=');
				if (pos3 != -1)
				{
					if (!_templateVars.TryGetValue(tag.Substring(0, pos3), out value))
						value = tag.Substring(pos3 + 1);
				}
				else
				{
					if (!_templateVars.TryGetValue(tag, out value))
						value = "";
				}
				buffer.Append(value);
				pos1 = pos2 + 1;
			}
			if (pos1 < template.Length)
				buffer.Append(template, pos1, template.Length - pos1);

			return buffer.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		public TextFormat textFormat
		{
			get
			{
				return _textField.textFormat;
			}
			set
			{
				_textField.textFormat = value;
				if (!underConstruct)
					UpdateSize();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get
			{
				return _textField.textFormat.color;
			}
			set
			{
				TextFormat tf = _textField.textFormat;
				tf.color = value;
				_textField.textFormat = tf;
				UpdateGear(4);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public AlignType align
		{
			get { return _textField.align; }
			set { _textField.align = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public VertAlignType verticalAlign
		{
			get { return _textField.verticalAlign; }
			set { _textField.verticalAlign = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool singleLine
		{
			get { return _textField.singleLine; }
			set { _textField.singleLine = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int stroke
		{
			get { return _textField.stroke; }
			set { _textField.stroke = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Color strokeColor
		{
			get { return _textField.strokeColor; }
			set
			{
				_textField.strokeColor = value;
				UpdateGear(4);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 shadowOffset
		{
			get { return _textField.shadowOffset; }
			set { _textField.shadowOffset = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool UBBEnabled
		{
			get { return _ubbEnabled; }
			set { _ubbEnabled = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public AutoSizeType autoSize
		{
			get { return _textField.autoSize; }
			set
			{
				_textField.autoSize = value;
				if (value == AutoSizeType.Both)
				{
					_textField.wordWrap = false;

					if (!underConstruct)
						this.SetSize(_textField.textWidth, _textField.textHeight);
				}
				else
				{
					_textField.wordWrap = true;

					if (value == AutoSizeType.Height)
					{
						if (!underConstruct)
							this.height = _textField.textHeight;
					}
					else
						displayObject.SetSize(this.width, this.height);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float textWidth
		{
			get { return _textField.textWidth; }
		}

		/// <summary>
		/// 
		/// </summary>
		public float textHeight
		{
			get { return _textField.textHeight; }
		}

		protected void UpdateSize()
		{
			if (_updatingSize)
				return;

			_updatingSize = true;

			if (_textField.autoSize == AutoSizeType.Both)
				this.size = displayObject.size;
			else if (_textField.autoSize == AutoSizeType.Height)
				this.height = displayObject.height;

			_updatingSize = false;
		}

		override protected void HandleSizeChanged()
		{
			if (_updatingSize)
				return;

			if (underConstruct)
				displayObject.SetSize(this.width, this.height);
			else if (_textField.autoSize != AutoSizeType.Both)
			{
				if (_textField.autoSize == AutoSizeType.Height)
				{
					displayObject.width = this.width;//先调整宽度，让文本重排
					if (this._text != string.Empty) //文本为空时，1是本来就不需要调整， 2是为了防止改掉文本为空时的默认高度，造成关联错误
						SetSizeDirectly(this.width, displayObject.height);
				}
				else
					displayObject.SetSize(this.width, this.height);
			}
		}

		override public void Setup_BeforeAdd(XML xml)
		{
			base.Setup_BeforeAdd(xml);

			TextFormat tf = _textField.textFormat;

			string str;
			str = xml.GetAttribute("font");
			if (str != null)
				tf.font = str;

			str = xml.GetAttribute("fontSize");
			if (str != null)
				tf.size = int.Parse(str);

			str = xml.GetAttribute("color");
			if (str != null)
				tf.color = ToolSet.ConvertFromHtmlColor(str);

			str = xml.GetAttribute("align");
			if (str != null)
				this.align = FieldTypes.ParseAlign(str);

			str = xml.GetAttribute("vAlign");
			if (str != null)
				this.verticalAlign = FieldTypes.ParseVerticalAlign(str);

			str = xml.GetAttribute("leading");
			if (str != null)
				tf.lineSpacing = int.Parse(str);

			str = xml.GetAttribute("letterSpacing");
			if (str != null)
				tf.letterSpacing = int.Parse(str);

			_ubbEnabled = xml.GetAttributeBool("ubb", false);

			str = xml.GetAttribute("autoSize");
			if (str != null)
				this.autoSize = FieldTypes.ParseAutoSizeType(str);

			tf.underline = xml.GetAttributeBool("underline", false);
			tf.italic = xml.GetAttributeBool("italic", false);
			tf.bold = xml.GetAttributeBool("bold", false);
			this.singleLine = xml.GetAttributeBool("singleLine", false);
			str = xml.GetAttribute("strokeColor");
			if (str != null)
			{
				this.strokeColor = ToolSet.ConvertFromHtmlColor(str);
				this.stroke = xml.GetAttributeInt("strokeSize", 1);
			}

			str = xml.GetAttribute("shadowColor");
			if (str != null)
			{
				this.strokeColor = ToolSet.ConvertFromHtmlColor(str);
				this.shadowOffset = xml.GetAttributeVector("shadowOffset");
			}

			if (xml.GetAttributeBool("vars"))
				_templateVars = new Dictionary<string, string>();

			_textField.textFormat = tf;
		}

		override public void Setup_AfterAdd(XML xml)
		{
			base.Setup_AfterAdd(xml);

			string str = xml.GetAttribute("text");
			if (str != null && str.Length > 0)
				this.text = str;
		}
	}
}
