using System;

namespace ExaminationSystem
{
    class Answer : ICloneable, IComparable<Answer>
    {
        public int AnswerId { get; set; }
        public string AnswerText { get; set; }

        public Answer() : this(0, "")
        {
        }

        public Answer(int answerId, string answerText)
        {
            AnswerId = answerId;
            AnswerText = answerText;
        }

        public object Clone()
        {
            return new Answer(AnswerId, AnswerText);
        }

        public int CompareTo(Answer other)
        {
            if (other == null) return 1;
            return AnswerId.CompareTo(other.AnswerId);
        }

        public override string ToString()
        {
            return AnswerId + ". " + AnswerText;
        }
    }

    abstract class Question : ICloneable, IComparable<Question>
    {
        public string Header { get; set; }
        public string Body { get; set; }
        public int Mark { get; set; }
        public Answer[] AnswerList { get; set; }
        public Answer RightAnswer { get; set; }

        protected Question() : this("", "", 0)
        {
        }

        protected Question(string header, string body, int mark)
        {
            Header = header;
            Body = body;
            Mark = mark;
            AnswerList = new Answer[0];
            RightAnswer = null;
        }

        protected Question(string header, string body, int mark, Answer[] answerList, Answer rightAnswer) : this(header, body, mark)
        {
            AnswerList = answerList;
            RightAnswer = rightAnswer;
        }

        protected Answer[] CloneAnswers()
        {
            Answer[] copy = new Answer[AnswerList.Length];
            for (int i = 0; i < AnswerList.Length; i++)
                copy[i] = (Answer)AnswerList[i].Clone();
            return copy;
        }

        protected Answer FindById(Answer[] list, int id)
        {
            foreach (Answer a in list)
                if (a.AnswerId == id) return a;
            return null;
        }

        public abstract object Clone();

        public int CompareTo(Question other)
        {
            if (other == null) return 1;
            return Mark.CompareTo(other.Mark);
        }

        public void Show()
        {
            Console.WriteLine(Header + " (" + Mark + " Marks)");
            Console.WriteLine(Body);
            foreach (Answer a in AnswerList)
                Console.WriteLine("   " + a);
        }

        public override string ToString()
        {
            return Header + " | " + Body + " | Mark: " + Mark;
        }
    }

    class TrueFalseQuestion : Question
    {
        public TrueFalseQuestion() : base()
        {
            AnswerList = new Answer[] { new Answer(1, "True"), new Answer(2, "False") };
            RightAnswer = AnswerList[0];
        }

        public TrueFalseQuestion(string header, string body, int mark, bool correctValue) : base(header, body, mark)
        {
            AnswerList = new Answer[] { new Answer(1, "True"), new Answer(2, "False") };
            RightAnswer = correctValue ? AnswerList[0] : AnswerList[1];
        }

        public override object Clone()
        {
            TrueFalseQuestion q = new TrueFalseQuestion(Header, Body, Mark, true);
            q.AnswerList = CloneAnswers();
            q.RightAnswer = FindById(q.AnswerList, RightAnswer.AnswerId);
            return q;
        }

        public override string ToString()
        {
            return "True/False: " + base.ToString();
        }
    }

    class MCQQuestion : Question
    {
        public MCQQuestion() : base()
        {
        }

        public MCQQuestion(string header, string body, int mark, Answer[] answerList, int rightAnswerId) : base(header, body, mark)
        {
            AnswerList = answerList;
            RightAnswer = FindById(answerList, rightAnswerId);
        }

        public override object Clone()
        {
            Answer[] copy = CloneAnswers();
            return new MCQQuestion(Header, Body, Mark, copy, RightAnswer.AnswerId);
        }

        public override string ToString()
        {
            return "MCQ: " + base.ToString();
        }
    }

    class Subject : ICloneable, IComparable<Subject>
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public Exam Exam { get; set; }

        public Subject() : this(0, "")
        {
        }

        public Subject(int subjectId, string subjectName)
        {
            SubjectId = subjectId;
            SubjectName = subjectName;
        }

        public void CreateExam()
        {
            Console.WriteLine("Choose exam type: 1) Final  2) Practical");
            int type = Input.ReadInt("Type: ", 1, 2);
            int time = Input.ReadInt("Exam time (minutes): ", 1, 1000);
            int count = Input.ReadInt("Number of questions: ", 1, 100);

            if (type == 1)
                Exam = new FinalExam(time, count, this);
            else
                Exam = new PracticalExam(time, count, this);

            for (int i = 0; i < count; i++)
            {
                Console.WriteLine();
                Console.WriteLine("--- Question " + (i + 1) + " ---");
                int qType = 2;
                if (type == 1)
                {
                    Console.WriteLine("Question type: 1) True or False  2) MCQ");
                    qType = Input.ReadInt("Type: ", 1, 2);
                }

                string header = Input.ReadText("Header: ");
                string body = Input.ReadText("Body: ");
                int mark = Input.ReadInt("Mark: ", 1, 1000);

                if (qType == 1)
                {
                    int right = Input.ReadInt("Right answer (1 = True, 2 = False): ", 1, 2);
                    Exam.Questions[i] = new TrueFalseQuestion(header, body, mark, right == 1);
                }
                else
                {
                    int n = Input.ReadInt("Number of answers: ", 2, 10);
                    Answer[] list = new Answer[n];
                    for (int j = 0; j < n; j++)
                        list[j] = new Answer(j + 1, Input.ReadText("Answer " + (j + 1) + ": "));
                    int right = Input.ReadInt("Right answer id: ", 1, n);
                    Exam.Questions[i] = new MCQQuestion(header, body, mark, list, right);
                }
            }
        }

        public object Clone()
        {
            Subject s = new Subject(SubjectId, SubjectName);
            if (Exam != null)
            {
                s.Exam = (Exam)Exam.Clone();
                s.Exam.Subject = s;
            }
            return s;
        }

        public int CompareTo(Subject other)
        {
            if (other == null) return 1;
            return SubjectId.CompareTo(other.SubjectId);
        }

        public override string ToString()
        {
            return "Subject Id: " + SubjectId + ", Name: " + SubjectName;
        }
    }

    abstract class Exam : ICloneable, IComparable<Exam>
    {
        public int Time { get; set; }
        public int NumberOfQuestions { get; set; }
        public Question[] Questions { get; set; }
        public int[] StudentAnswers { get; set; }
        public Subject Subject { get; set; }

        protected Exam() : this(0, 0, null)
        {
        }

        protected Exam(int time, int numberOfQuestions, Subject subject)
        {
            Time = time;
            NumberOfQuestions = numberOfQuestions;
            Subject = subject;
            Questions = new Question[numberOfQuestions];
            StudentAnswers = new int[numberOfQuestions];
        }

        protected void CopyQuestionsTo(Exam target)
        {
            for (int i = 0; i < Questions.Length; i++)
            {
                if (Questions[i] != null)
                    target.Questions[i] = (Question)Questions[i].Clone();
                target.StudentAnswers[i] = StudentAnswers[i];
            }
        }

        protected void AskQuestions()
        {
            Console.WriteLine();
            Console.WriteLine("Subject: " + Subject.SubjectName + " | Time: " + Time + " minutes");
            for (int i = 0; i < Questions.Length; i++)
            {
                Console.WriteLine();
                Console.WriteLine("Q" + (i + 1));
                Questions[i].Show();
                StudentAnswers[i] = Input.ReadInt("Your answer id: ", 1, Questions[i].AnswerList.Length);
            }
        }

        public abstract void ShowExam();

        public abstract object Clone();

        public int CompareTo(Exam other)
        {
            if (other == null) return 1;
            return Time.CompareTo(other.Time);
        }

        public override string ToString()
        {
            return GetType().Name + " | Time: " + Time + " | Questions: " + NumberOfQuestions;
        }
    }

    class FinalExam : Exam
    {
        public FinalExam() : base()
        {
        }

        public FinalExam(int time, int numberOfQuestions, Subject subject) : base(time, numberOfQuestions, subject)
        {
        }

        public override void ShowExam()
        {
            AskQuestions();

            int grade = 0;
            int total = 0;

            Console.WriteLine();
            Console.WriteLine("========== Final Exam Result ==========");
            for (int i = 0; i < Questions.Length; i++)
            {
                Question q = Questions[i];
                total += q.Mark;
                Console.WriteLine();
                Console.WriteLine("Q" + (i + 1) + ") " + q.Header + " (" + q.Mark + " Marks)");
                Console.WriteLine(q.Body);
                foreach (Answer a in q.AnswerList)
                    Console.WriteLine("   " + a);
                Console.WriteLine("Your answer: " + StudentAnswers[i]);
                if (StudentAnswers[i] == q.RightAnswer.AnswerId)
                    grade += q.Mark;
            }

            Console.WriteLine();
            Console.WriteLine("Grade: " + grade + " / " + total);
        }

        public override object Clone()
        {
            FinalExam e = new FinalExam(Time, NumberOfQuestions, Subject);
            CopyQuestionsTo(e);
            return e;
        }
    }

    class PracticalExam : Exam
    {
        public PracticalExam() : base()
        {
        }

        public PracticalExam(int time, int numberOfQuestions, Subject subject) : base(time, numberOfQuestions, subject)
        {
        }

        public override void ShowExam()
        {
            AskQuestions();

            Console.WriteLine();
            Console.WriteLine("========== Right Answers ==========");
            for (int i = 0; i < Questions.Length; i++)
            {
                Console.WriteLine("Q" + (i + 1) + ") " + Questions[i].Body);
                Console.WriteLine("Right answer: " + Questions[i].RightAnswer);
            }
        }

        public override object Clone()
        {
            PracticalExam e = new PracticalExam(Time, NumberOfQuestions, Subject);
            CopyQuestionsTo(e);
            return e;
        }
    }

    static class Input
    {
        public static int ReadInt(string message, int min, int max)
        {
            int value;
            while (true)
            {
                Console.Write(message);
                if (int.TryParse(Console.ReadLine(), out value) && value >= min && value <= max)
                    return value;
                Console.WriteLine("Invalid input. Enter a number between " + min + " and " + max + ".");
            }
        }

        public static string ReadText(string message)
        {
            string text;
            do
            {
                Console.Write(message);
                text = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(text));
            return text;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Subject subject = new Subject(1, "Object Oriented Programming");
            subject.CreateExam();
            subject.Exam.ShowExam();
            Console.ReadKey();
        }
    }
}