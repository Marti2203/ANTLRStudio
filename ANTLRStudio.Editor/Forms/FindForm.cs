using System;
using Eto.Forms;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ANTLRStudio.Editor.Text;
using FastColoredTextBoxNS;
using Range = ANTLRStudio.Editor.Text.Range;
using System.ComponentModel;
using Eto.Drawing;

namespace ANTLRStudio.Editor.Forms
{
    public class FindForm : Form
    {
        bool firstSearch = true;
        Place startPlace;
        FastColoredTextBox tb;

        public FindForm(FastColoredTextBox tb)
        {
            InitializeComponent();
            this.tb = tb;
        }

        private void BtClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtFindNext_Click(object sender, EventArgs e)
        {
            FindNext(tbFind.Text);
        }

        public virtual void FindNext(string pattern)
        {
            try
            {
                RegexOptions opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
                if (!cbRegex.Checked)
                    pattern = Regex.Escape(pattern);
                if (cbWholeWord.Checked)
                    pattern = "\\b" + pattern + "\\b";
                //
                Range range = tb.Selection.Clone();
                range.Normalize();
                //
                if (firstSearch)
                {
                    startPlace = range.Start;
                    firstSearch = false;
                }
                //
                range.Start = range.End;
                if (range.Start >= startPlace)
                    range.End = new Place(tb.GetLineLength(tb.LinesCount - 1), tb.LinesCount - 1);
                else
                    range.End = startPlace;
                //
                foreach (var r in range.GetRangesByLines(pattern, opt))
                {
                    tb.Selection = r;
                    tb.DoSelectionVisible();
                    tb.Invalidate();
                    return;
                }
                //
                if (range.Start >= startPlace && startPlace > Place.Empty)
                {
                    tb.Selection.Start = new Place(0, 0);
                    FindNext(pattern);
                    return;
                }
                MessageBox.Show("Not found");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TbFind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                btFindNext.PerformClick();
                e.Handled = true;
                return;
            }
            if (e.KeyChar == '\x1b')
            {
                Hide();
                e.Handled = true;
                return;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.tb.Focus();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Keys.Escape)
            {
                this.Close();
            }
            base.OnKeyDown(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            tbFind.Focus();
            ResetSerach();
        }

        void ResetSerach()
        {
            firstSearch = true;
        }

        private void CbMatchCase_CheckedChanged(object sender, EventArgs e)
        {
            ResetSerach();
        }


        private void InitializeComponent()
        {
            this.btClose = new Button
            {
                //Location = new System.Drawing.Point(273, 73),
                ID = "btClose",
                Size = new Size(75, 23),
                TabIndex = 5,
                Text = "Close"
            };
            btClose.Click += BtClose_Click;
            this.btFindNext = new Button
            {
                //Location = new System.Drawing.Point(192, 73),
                ID = "btFindNext",
                Size = new Size(75, 23),
                TabIndex = 4,
                Text = "Find next"
            };
            btFindNext.Click += this.BtFindNext_Click;
            this.tbFind = new TextBox
            {
                //Location = new System.Drawing.Point(42, 12),
                ID = "tbFind",
                Size = new Size(306, 20),
                TabIndex = 0
            };
            this.tbFind.TextChanged += this.CbMatchCase_CheckedChanged;
            this.tbFind.KeyPress += this.tbFind_KeyPress;
            this.cbRegex = new CheckBox
            {
                //AutoSize = true,
                //Location = new System.Drawing.Point(249, 38),
                ID = "cbRegex",
                Size = new Size(57, 17),
                TabIndex = 3,
                Text = "Regex",
            };
            this.cbRegex.CheckedChanged += this.CbMatchCase_CheckedChanged;
            this.cbMatchCase = new CheckBox
            {
                //AutoSize = true,
                //Location = new System.Drawing.Point(42, 38),
                ID = "cbMatchCase",
                Size = new Size(82, 17),
                TabIndex = 1,
                Text = "Match case",
                //UseVisualStyleBackColor = true
            };
            this.cbMatchCase.CheckedChanged += this.CbMatchCase_CheckedChanged;
            this.label1 = new Label
            {
                //AutoSize = true,
                //Location = new System.Drawing.Point(6, 15),
                ID = "label1",
                Size = new Size(33, 13),
                TabIndex = 5,
                Text = "Find: "
            };
            this.cbWholeWord = new CheckBox
            {
                //AutoSize = true,
                //Location = new System.Drawing.Point(130, 38),
                ID = "cbWholeWord",
                Size = new Size(113, 17),
                TabIndex = 2,
                Text = "Match whole word",
                //UseVisualStyleBackColor = true
            };
            this.cbWholeWord.CheckedChanged += this.CbMatchCase_CheckedChanged;
            this.SuspendLayout();

            // 
            // cbWholeWord
            // 
            // 
            // FindForm
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(360, 108);
            this.Controls.Add(this.cbWholeWord);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbMatchCase);
            this.Controls.Add(this.cbRegex);
            this.Controls.Add(this.tbFind);
            this.Controls.Add(this.btFindNext);
            this.Controls.Add(this.btClose);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ID = "FindForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Find";
            this.TopMost = true;
            this.FormClosing += this.FindForm_FormClosing;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Button btClose;
        private Button btFindNext;
        private CheckBox cbRegex;
        private CheckBox cbMatchCase;
        private Label label1;
        private CheckBox cbWholeWord;
        public TextBox tbFind;
    }
}
