using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bruh.Model.Models
{
    public class FixedPieSeries : PieSeries
    {
        /// <summary>
        /// The actual points of the slices.
        /// </summary>
        private readonly List<IList<ScreenPoint>> slicePoints = new List<IList<ScreenPoint>>();

        /// <summary>
        /// The total value of all the pie slices.
        /// </summary>
        private double total;


        /// <summary>
        /// Gets the point on the series that is nearest the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="interpolate">Interpolate the series if this flag is set to <c>true</c> .</param>
        /// <returns>A TrackerHitResult for the current hit.</returns>
        public override TrackerHitResult GetNearestPoint(ScreenPoint point, bool interpolate)
        {
            for (int i = 0; i < this.slicePoints.Count; i++)
            {
                if (ScreenPointHelper.IsPointInPolygon(point, this.slicePoints[i]))
                {
                    var slice = this.Slices[i];
                    var item = this.GetItem(i);
                    return new TrackerHitResult
                    {
                        Series = this,
                        Position = point,
                        Item = item,
                        Index = i,
                        Text = StringHelper.Format(this.ActualCulture, this.TrackerFormatString, slice, this.Title, slice.Label, slice.Value, slice.Value / this.total)
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// Renders the series on the specified render context.
        /// </summary>
        /// <param name="rc">The rendering context.</param>
        public override void Render(IRenderContext rc)
        {
            this.slicePoints.Clear();

            if (this.Slices.Count == 0)
            {
                return;
            }

            this.total = this.Slices.Sum(slice => slice.Value);
            if (Math.Abs(this.total) < double.Epsilon)
            {
                return;
            }

            double radius = Math.Min(this.PlotModel.PlotArea.Width, this.PlotModel.PlotArea.Height) / 2;

            double outerRadius = radius * (this.Diameter - this.ExplodedDistance);
            double innerRadius = radius * this.InnerDiameter;

            double angle = this.StartAngle;
            var midPoint = new ScreenPoint(
                (this.PlotModel.PlotArea.Left + this.PlotModel.PlotArea.Right) * 0.5, (this.PlotModel.PlotArea.Top + this.PlotModel.PlotArea.Bottom) * 0.5);

            foreach (var slice in this.Slices)
            {
                var outerPoints = new List<ScreenPoint>();
                var innerPoints = new List<ScreenPoint>();

                double sliceAngle = slice.Value / this.total * this.AngleSpan;
                double endAngle = angle + sliceAngle;
                double explodedRadius = slice.IsExploded ? this.ExplodedDistance * radius : 0.0;

                double midAngle = angle + (sliceAngle / 2);
                double midAngleRadians = midAngle * Math.PI / 180;
                var mp = new ScreenPoint(
                    midPoint.X + (explodedRadius * Math.Cos(midAngleRadians)),
                    midPoint.Y + (explodedRadius * Math.Sin(midAngleRadians)));

                // Create the pie sector points for both outside and inside arcs
                while (true)
                {
                    bool stop = false;
                    if (angle >= endAngle)
                    {
                        angle = endAngle;
                        stop = true;
                    }

                    double a = angle * Math.PI / 180;
                    var op = new ScreenPoint(mp.X + (outerRadius * Math.Cos(a)), mp.Y + (outerRadius * Math.Sin(a)));
                    outerPoints.Add(op);
                    var ip = new ScreenPoint(mp.X + (innerRadius * Math.Cos(a)), mp.Y + (innerRadius * Math.Sin(a)));
                    if (innerRadius + explodedRadius > 0)
                    {
                        innerPoints.Add(ip);
                    }

                    if (stop)
                    {
                        break;
                    }

                    angle += this.AngleIncrement;
                }

                innerPoints.Reverse();
                if (innerPoints.Count == 0)
                {
                    innerPoints.Add(mp);
                }

                innerPoints.Add(outerPoints[0]);

                var points = outerPoints;
                points.AddRange(innerPoints);

                rc.DrawPolygon(points, slice.ActualFillColor, this.Stroke, this.StrokeThickness, this.EdgeRenderingMode, null, LineJoin.Bevel);

                // keep the point for hit testing
                this.slicePoints.Add(points);

                // Render label outside the slice
                if (this.OutsideLabelFormat != null)
                {
                    string label = string.Format(
                        this.OutsideLabelFormat, slice.Value, slice.Label, slice.Value / this.total * 100);
                    int sign = Math.Sign(Math.Cos(midAngleRadians));

                    // tick points
                    var tp0 = new ScreenPoint(
                        mp.X + ((outerRadius + this.TickDistance) * Math.Cos(midAngleRadians)),
                        mp.Y + ((outerRadius + this.TickDistance) * Math.Sin(midAngleRadians)));
                    var tp1 = new ScreenPoint(
                        tp0.X + (this.TickRadialLength * Math.Cos(midAngleRadians)),
                        tp0.Y + (this.TickRadialLength * Math.Sin(midAngleRadians)));
                    var tp2 = new ScreenPoint(tp1.X + (this.TickHorizontalLength * sign), tp1.Y);

                    // draw the tick line with the same color as the text
                    rc.DrawLine(new[] { tp0, tp1, tp2 }, this.ActualTextColor, 1, this.EdgeRenderingMode, null, LineJoin.Bevel);

                    // label
                    var labelPosition = new ScreenPoint(tp2.X + (this.TickLabelDistance * sign), tp2.Y);
                    rc.DrawText(
                        labelPosition,
                        label,
                        this.ActualTextColor,
                        this.ActualFont,
                        this.ActualFontSize,
                        this.ActualFontWeight,
                        0,
                        sign > 0 ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                        VerticalAlignment.Middle);
                }
            }

            angle = this.StartAngle;

            foreach (var slice in this.Slices)
            {
                double sliceAngle = slice.Value / this.total * this.AngleSpan;
                double endAngle = angle + sliceAngle;
                double explodedRadius = slice.IsExploded ? this.ExplodedDistance * radius : 0.0;

                double midAngle = angle + (sliceAngle / 2);
                double midAngleRadians = midAngle * Math.PI / 180;
                var mp = new ScreenPoint(
                    midPoint.X + (explodedRadius * Math.Cos(midAngleRadians)),
                    midPoint.Y + (explodedRadius * Math.Sin(midAngleRadians)));

                // Create the pie sector points for both outside and inside arcs
                while (true)
                {
                    bool stop = false;
                    if (angle >= endAngle)
                    {
                        angle = endAngle;
                        stop = true;
                    }

                    if (stop)
                    {
                        break;
                    }

                    angle += this.AngleIncrement;
                }

                // Render a label inside the slice
                if (this.InsideLabelFormat != null && !this.InsideLabelColor.IsUndefined())
                {
                    string label = string.Format(
                        this.InsideLabelFormat, slice.Value, slice.Label, slice.Value / this.total * 100);
                    double r = (innerRadius * (1 - this.InsideLabelPosition)) + (outerRadius * this.InsideLabelPosition);
                    var labelPosition = new ScreenPoint(
                        mp.X + (r * Math.Cos(midAngleRadians)), mp.Y + (r * Math.Sin(midAngleRadians)));
                    double textAngle = 0;
                    if (this.AreInsideLabelsAngled)
                    {
                        textAngle = midAngle;
                        if (Math.Cos(midAngleRadians) < 0)
                        {
                            textAngle += 180;
                        }
                    }

                    var actualInsideLabelColor = this.InsideLabelColor.IsAutomatic() ? this.ActualTextColor : this.InsideLabelColor;

                    rc.DrawText(
                        labelPosition,
                        label,
                        actualInsideLabelColor,
                        this.ActualFont,
                        this.ActualFontSize,
                        this.ActualFontWeight,
                        textAngle,
                        HorizontalAlignment.Center,
                        VerticalAlignment.Middle);
                }
            }
        }
    }
}
