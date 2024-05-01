using System;
using System.IO;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace CompilePrinciple_Ex1
{
    class Program
    {
        static string MIDLInputFilePath = "/Users/jinshengkai/Projects/CompilePrinciple_Ex1/CompilePrinciple_Ex1/test/test.idl";
        static string ASTOutputFilePath = "/Users/jinshengkai/Projects/CompilePrinciple_Ex1/CompilePrinciple_Ex1/test/AST.txt";
        static string CodeOutputFilePath = "/Users/jinshengkai/Projects/CompilePrinciple_Ex1/CompilePrinciple_Ex1/output/test.hxx";
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
            string content = File.ReadAllText(MIDLInputFilePath);
            ICharStream stream = CharStreams.fromString(content);
            ITokenSource lexer = new MIDLLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            MIDLParser parser = new MIDLParser(tokens);
            IParseTree tree = parser.specification();
            ASTGenerator astGenerator = new ASTGenerator();
            ASTNode node = tree.Accept(astGenerator);

            File.WriteAllText(ASTOutputFilePath, AST(node));

            ASTAnalyzer analyzer = new ASTAnalyzer();
            analyzer.Start(node);

            File.WriteAllText(CodeOutputFilePath, node.ToCppCode(0));

            Console.WriteLine("Completed.");
        }
    }
}
