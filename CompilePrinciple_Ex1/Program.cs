using System;
using System.IO;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace CompilePrinciple_Ex1
{
    class Program
    {
        static string SyntaxTree(MIDLParser parser, IParseTree tree)
        {
            string output = "";
            Stack<(IParseTree, int)> nodes = new Stack<(IParseTree, int)>();
            nodes.Push((tree, 0));
            while (nodes.Count > 0)
            {
                (IParseTree node, int level) = nodes.Pop();

                string nodeText = "";
                if (node is RuleContext ruleContext)
                {
                    nodeText = parser.RuleNames[ruleContext.RuleIndex];
                }
                else if (node is ITerminalNode terminal)
                {
                    nodeText = '\'' + terminal.GetText() + '\'';
                }


                for (int i = 0; i < level; i++)
                    nodeText = '\t' + nodeText;

                output += nodeText + '\n';

                int count = node.ChildCount;
                for (int i = count - 1; i >= 0; i--)
                {
                    nodes.Push((node.GetChild(i), level + 1));
                }
            }
            return output;
        }
        static string AST(ASTNode tree)
        {
            string output = "";
            Stack<(ASTNode, int)> nodes = new Stack<(ASTNode, int)>();
            nodes.Push((tree, 0));
            while (nodes.Count > 0)
            {
                (ASTNode node, int level) = nodes.Pop();

                string nodeText = node.ToString();

                for (int i = 0; i < level; i++)
                    nodeText = '\t' + nodeText;

                output += nodeText + '\n';

                for (int i = node.Childs.Count - 1; i >= 0; i--)
                    nodes.Push((node.Childs[i], level + 1));
            }
            return output;
        }
        static void Main(string[] args)
        {
            string filePath = "/Users/jinshengkai/Projects/CompilePrinciple_Ex1/CompilePrinciple_Ex1/test/test.idl";
            string content = File.ReadAllText(filePath);
            ICharStream stream = CharStreams.fromString(content);
            ITokenSource lexer = new MIDLLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            MIDLParser parser = new MIDLParser(tokens);
            IParseTree tree = parser.specification();
            ASTGenerator astGenerator = new ASTGenerator();
            ASTNode node = tree.Accept(astGenerator);

            string outputPath = "/Users/jinshengkai/Projects/CompilePrinciple_Ex1/CompilePrinciple_Ex1/test/AST.txt";
            string output = AST(node);
            File.WriteAllText(outputPath, output);

            ASTAnalyzer analyzer = new ASTAnalyzer();
            analyzer.Start(node);

            Console.WriteLine("Completed.");
        }
    }
}
