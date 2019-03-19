using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.service;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    class ComponentDemoTabData {
        public ComponentDemoTabData(
            Widget demoWidget = null,
            string exampleCodeTag = null,
            string description = null,
            string tabName = null,
            string documentationUrl = null
        ) {
            this.demoWidget = demoWidget;
            this.exampleCodeTag = exampleCodeTag;
            this.description = description;
            this.tabName = tabName;
            this.documentationUrl = documentationUrl;
        }

        public readonly Widget demoWidget;
        public readonly string exampleCodeTag;
        public readonly string description;
        public readonly string tabName;
        public readonly string documentationUrl;

        public static bool operator ==(ComponentDemoTabData left, ComponentDemoTabData right) {
            return right.tabName == left.tabName
                   && right.description == left.description
                   && right.documentationUrl == left.documentationUrl;
        }

        public static bool operator !=(ComponentDemoTabData left, ComponentDemoTabData right) {
            return right.tabName != left.tabName
                   || right.description != left.description
                   || right.documentationUrl != left.documentationUrl;
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.tabName.GetHashCode();
                hashCode = (hashCode * 397) ^ this.description.GetHashCode();
                hashCode = (hashCode * 397) ^ this.documentationUrl.GetHashCode();
                return hashCode;
            }
        }
    }


    class TabbedComponentDemoScaffold : StatelessWidget {
        public TabbedComponentDemoScaffold(
            string title = null,
            List<ComponentDemoTabData> demos = null,
            List<Widget> actions = null
        ) {
            this.title = title;
            this.demos = demos;
            this.actions = actions;
        }

        public readonly List<ComponentDemoTabData> demos;
        public readonly string title;
        public readonly List<Widget> actions;

        void _showExampleCode(BuildContext context) {
            string tag = this.demos[DefaultTabController.of(context).index].exampleCodeTag;
            if (tag != null) {
                Navigator.push(context, new MaterialPageRoute(
                    builder: (BuildContext _context) => new FullScreenCodeDialog(exampleCodeTag: tag)
                ));
            }
        }

        void _showApiDocumentation(BuildContext context) {
            string url = this.demos[DefaultTabController.of(context).index].documentationUrl;
            if (url != null) {
                // TODO: find Unity equivalent
                // Open the URL in browser
                // launch(url, forceWebView: true);
            }
        }

        public override Widget build(BuildContext context) {
            List<Widget> actions = this.actions ?? new List<Widget> { };
            actions.AddRange(
                new List<Widget> {
                    new Builder(
                        builder: (BuildContext _context) => {
                            return new IconButton(
                                icon: new Icon(Icons.library_books),
                                onPressed: () => this._showApiDocumentation(_context)
                            );
                        }
                    ),
                    new Builder(
                        builder: (BuildContext _context) => {
                            return new IconButton(
                                icon: new Icon(Icons.code),
                                tooltip: "Show example code",
                                onPressed: () => this._showExampleCode(_context)
                            );
                        }
                    )
                }
            );
            return new DefaultTabController(
                length: this.demos.Count,
                child: new Scaffold(
                    appBar: new AppBar(
                        title: new Text(this.title),
                        actions: actions,
                        bottom: new TabBar(
                            isScrollable: true,
                            tabs: this.demos.Select<ComponentDemoTabData, Widget>(
                                    (ComponentDemoTabData data) => new Tab(text: data.tabName))
                                .ToList()
                        )
                    ),
                    body: new TabBarView(
                        children: this.demos.Select<ComponentDemoTabData, Widget>((ComponentDemoTabData demo) => {
                            return new SafeArea(
                                top: false,
                                bottom: false,
                                child: new Column(
                                    children: new List<Widget> {
                                        new Padding(
                                            padding: EdgeInsets.all(16.0f),
                                            child: new Text(demo.description,
                                                style: Theme.of(context).textTheme.subhead
                                            )
                                        ),
                                        new Expanded(child: demo.demoWidget)
                                    }
                                )
                            );
                        }).ToList()
                    )
                )
            );
        }
    }


    class FullScreenCodeDialog : StatefulWidget {
        public FullScreenCodeDialog(Key key = null, string exampleCodeTag = null) : base(key: key) {
            this.exampleCodeTag = exampleCodeTag;
        }

        public readonly string exampleCodeTag;

        public override State createState() {
            return new FullScreenCodeDialogState();
        }
    }

    class FullScreenCodeDialogState : State<FullScreenCodeDialog> {
        public FullScreenCodeDialogState() {
        }

        string _exampleCode;

        public override void didChangeDependencies() {
            new ExampleCodeParser().getExampleCode(this.widget.exampleCodeTag, DefaultAssetBundle.of(this.context)).Then(
                (string code) => {
                    if (this.mounted) {
                        this.setState(() => { this._exampleCode = code ?? "Example code not found"; });
                    }
                });
            base.didChangeDependencies();
        }

        public override Widget build(BuildContext context) {
            SyntaxHighlighterStyle style = Theme.of(context).brightness == Brightness.dark
                ? SyntaxHighlighterStyle.darkThemeStyle()
                : SyntaxHighlighterStyle.lightThemeStyle();

            Widget body;
            if (this._exampleCode == null) {
                body = new Center(
                    child: new CircularProgressIndicator()
                );
            }
            else {
                body = new SingleChildScrollView(
                    child: new Padding(
                        padding: EdgeInsets.all(16.0f),
                        child: new RichText(
                            text: new TextSpan(
                                style: new TextStyle(fontFamily: "monospace", fontSize: 10.0f),
                                children: new List<TextSpan> {
                                    new DartSyntaxHighlighter(style).format(this._exampleCode)
                                }
                            )
                        )
                    )
                );
            }

            return new Scaffold(
                appBar: new AppBar(
                    leading: new IconButton(
                        icon: new Icon(
                            Icons.clear
                        ),
                        onPressed: () => { Navigator.pop(context); }
                    ),
                    title: new Text("Example code")
                ),
                body: body
            );
        }
    }
}