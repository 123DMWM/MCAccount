using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO;
using ServiceStack.Text;

namespace MCAccount {
	public partial class Form1 : Form {
		public Form1() {
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e) {
			updatePage();
			
		}
		private void button1_Key(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Enter) {
				updatePage();
			}
		}
		private void updatePage() {
			String uuid = getUUID(textBox1.Text);
			if (uuid == null) {
				MessageBox.Show("No player by that name");
				return;
			}
			List<NameChange> names = JsonConvert.DeserializeObject<List<NameChange>>(getNames(uuid));
			textBox2.ResetText();
			DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			for (int i = 0; i < names.Count; i++) {
				DateTime changedAt = epoch.AddMilliseconds(double.Parse(names[i].changedToAt)).ToLocalTime();
				textBox2.Text += (i + 1) + ". " + names[i].name + Environment.NewLine + (changedAt == epoch.ToLocalTime() ? "" : "	" + 
					changedAt.ToShortDateString() + " at " + changedAt.ToLongTimeString() + Environment.NewLine);
			}
			pictureBox1.Image = getSkin(uuid);

		}
		private String getUUID(string name) {
			JsonObject result = JsonObject.Parse((new WebClient()).DownloadString("https://api.mojang.com/users/profiles/minecraft/" + name));
			try {
				return result.Get("id");
			} catch {
				try {
					result = JsonObject.Parse((new WebClient()).DownloadString("https://api.mojang.com/users/profiles/minecraft/" + name + "?at=0"));
					return result.Get("id");
				} catch (Exception ex) {
					return null;
				}
			}
		}
		private String getNames(string uuid) {
			return new StreamReader(new WebClient().OpenRead("https://api.mojang.com/user/profiles/" + uuid + "/names")).ReadToEnd();
		}
		private Image getSkin(string uuid) {
			WebClient client = new WebClient();
			Stream stream;
			try {
				stream = client.OpenRead("http://visage.gameminers.com/full/" + pictureBox1.Size.Height + "/" + uuid);
				setSkinButton("1");
				return Image.FromStream(stream);
			} catch {
				try {
					stream = client.OpenRead("https://crafatar.com/renders/body/" + uuid + "?helm&scale=7");
					setSkinButton("2");
					return Image.FromStream(stream);
				} catch {
					MessageBox.Show("Failed to get skin. Main and backup websites might be down!");
					return null;
				}
			}
			
		}
		string skinURL;
		private void setSkinButton(string urlNum) {
			string urlHost = "Visage";
			string urlLink = "http://visage.gameminers.com/";
			switch (urlNum) {
				case "2":
					urlHost = "Crafatar";
					urlLink = "https://crafatar.com/";
					break;
				default:
					break;
			}
			button2.Text = "Skin rendered using " + urlHost;
			skinURL = urlLink;
		}

		private void button2_Click(object sender, EventArgs e) {
			System.Diagnostics.Process.Start(skinURL);

		}
	}
	class NameChange { public String name { get; set; } public String changedToAt = "0"; }
	class Skins { public String SKIN { get; set; } public String url { get; set; } }
}
