using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using AssetStudio;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzurLaneLive2DExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;
            foreach (var arg in args)
            {
                if (!File.Exists(arg))
                    continue;
                var path = Path.GetFullPath(arg);
                var assetsManager = new AssetsManager();
                assetsManager.LoadFiles(path);
                if (assetsManager.assetsFileList.Count == 0)
                {
                    continue;
                }
                var assets = assetsManager.assetsFileList[0].Objects.Values.ToList();
                var name = Path.GetFileName(path);
                var destPath = @"live2d\" + name + @"\";
                var destTexturePath = @"live2d\" + name + @"\textures\";
                var destAnimationPath = @"live2d\" + name + @"\motions\";
                Directory.CreateDirectory(destPath);
                Directory.CreateDirectory(destTexturePath);
                Directory.CreateDirectory(destAnimationPath);
                Console.WriteLine($"Extract {name}");
                //physics
                var physics = (TextAsset)(assets.First(x => x is TextAsset));
                File.WriteAllBytes($"{destPath}{physics.m_Name}.json", physics.m_Script);
                //moc
                var moc = assets.First(x => x is MonoBehaviour);
                foreach (var assetPreloadData in assets.Where(x => x is MonoBehaviour))
                {
                    if (assetPreloadData.byteSize > moc.byteSize)
                    {
                        moc = assetPreloadData;
                    }
                }
                var mocReader = moc.reader;
                mocReader.Reset();
                mocReader.Position += 28;
                mocReader.ReadAlignedString();
                var mocBuff = mocReader.ReadBytes(mocReader.ReadInt32());
                File.WriteAllBytes($"{destPath}{name}.moc3", mocBuff);
                //texture
                var textures = new SortedSet<string>();
                foreach (var texture2D in assets.OfType<Texture2D>())
                {
                    using (var bitmap = new Texture2DConverter(texture2D).ConvertToBitmap(true))
                    {
                        textures.Add($"textures/{texture2D.m_Name}.png");
                        bitmap.Save($"{destTexturePath}{texture2D.m_Name}.png", ImageFormat.Png);
                    }
                }
                //motions
                var motions = new List<string>();
                var animator = (Animator)assets.First(x => x is Animator);
                var animations = assets.OfType<AnimationClip>().ToArray();
                animator.m_GameObject.TryGet(out GameObject rootGameObject);
                var converter = new CubismMotion3Converter(rootGameObject, animations);
                foreach (ImportedKeyframedAnimation animation in converter.AnimationList)
                {
                    var json = new CubismMotion3Json
                    {
                        Version = 3,
                        Meta = new SerializableMeta
                        {
                            Duration = animation.Duration,
                            Fps = animation.SampleRate,
                            Loop = true,
                            CurveCount = animation.TrackList.Count,
                            UserDataCount = 0,
                            TotalUserDataSize = 0
                        },
                        Curves = new SerializableCurve[animation.TrackList.Count]
                    };
                    int totalSegmentCount = 1;
                    int totalPointCount = 1;
                    for (int i = 0; i < animation.TrackList.Count; i++)
                    {
                        var track = animation.TrackList[i];
                        json.Curves[i] = new SerializableCurve
                        {
                            Target = track.Target,
                            Id = track.Name,
                            Segments = new List<float> { 0f, track.Curve[0].value }
                        };
                        for (var j = 1; j < track.Curve.Count; j++)
                        {
                            var curve = track.Curve[j];
                            var preCurve = track.Curve[j - 1];
                            if (Math.Abs(curve.time - preCurve.time - 0.01f) < 0.0001f) //InverseSteppedSegment
                            {
                                var nextCurve = track.Curve[j + 1];
                                if (nextCurve.value == curve.value)
                                {
                                    json.Curves[i].Segments.Add(3f);
                                    json.Curves[i].Segments.Add(nextCurve.time);
                                    json.Curves[i].Segments.Add(nextCurve.value);
                                    j += 1;
                                    totalPointCount += 1;
                                    totalSegmentCount++;
                                    continue;
                                }
                            }
                            if (curve.inSlope == float.PositiveInfinity) //SteppedSegment
                            {
                                json.Curves[i].Segments.Add(2f);
                                json.Curves[i].Segments.Add(curve.time);
                                json.Curves[i].Segments.Add(curve.value);
                                totalPointCount += 1;
                            }
                            else if (preCurve.outSlope == 0f && Math.Abs(curve.inSlope) < 0.0001f) //LinearSegment
                            {
                                json.Curves[i].Segments.Add(0f);
                                json.Curves[i].Segments.Add(curve.time);
                                json.Curves[i].Segments.Add(curve.value);
                                totalPointCount += 1;
                            }
                            else //BezierSegment
                            {
                                var tangentLength = (curve.time - preCurve.time) / 3f;
                                json.Curves[i].Segments.Add(1f);
                                json.Curves[i].Segments.Add(preCurve.time + tangentLength);
                                json.Curves[i].Segments.Add(preCurve.outSlope * tangentLength + preCurve.value);
                                json.Curves[i].Segments.Add(curve.time - tangentLength);
                                json.Curves[i].Segments.Add(curve.value - curve.inSlope * tangentLength);
                                json.Curves[i].Segments.Add(curve.time);
                                json.Curves[i].Segments.Add(curve.value);
                                totalPointCount += 3;
                            }
                            totalSegmentCount++;
                        }
                    }

                    json.Meta.TotalSegmentCount = totalSegmentCount;
                    json.Meta.TotalPointCount = totalPointCount;

                    motions.Add($"motions/{animation.Name}.motion3.json");
                    File.WriteAllText($"{destAnimationPath}{animation.Name}.motion3.json", JsonConvert.SerializeObject(json, Formatting.Indented, new MyJsonConverter()));
                }
                //model
                var job = new JObject();
                var jarray = new JArray();
                var tempjob = new JObject();
                foreach (var motion in motions)
                {
                    tempjob["File"] = motion;
                    jarray.Add(tempjob);
                }
                job[""] = jarray;

                var model3 = new CubismModel3Json
                {
                    Version = 3,
                    FileReferences = new SerializableFileReferences
                    {
                        Moc = $"{name}.moc3",
                        Textures = textures.ToArray(),
                        Physics = $"{physics.m_Name}.json",
                        Motions = job
                    },
                    Groups = new[]
                    {
                        new SerializableGroup
                        {
                            Target = "Parameter",
                            Name = "LipSync",
                            Ids = new[] {"ParamMouthOpenY"}
                        },
                        new SerializableGroup
                        {
                            Target = "Parameter",
                            Name = "EyeBlink",
                            Ids = new[] {"ParamEyeLOpen", "ParamEyeROpen"}
                        }
                    }
                };
                File.WriteAllText($"{destPath}{name}.model3.json", JsonConvert.SerializeObject(model3, Formatting.Indented));
            }
            Console.WriteLine("Done!");
            Console.Read();
        }
    }
}
