import Foundation
import CoreGraphics
import ImageIO
import UniformTypeIdentifiers

func load(_ p: String) -> CGImage {
    let src = CGImageSourceCreateWithURL(URL(fileURLWithPath: p) as CFURL, nil)!
    return CGImageSourceCreateImageAtIndex(src, 0, nil)!
}
let a = CommandLine.arguments
let bg = load(a[1]), fig = load(a[2]); let out = a[3]
let W = Int(a[4])!, H = Int(a[5])!
let ctx = CGContext(data: nil, width: W, height: H, bitsPerComponent: 8, bytesPerRow: 0,
                    space: CGColorSpaceCreateDeviceRGB(),
                    bitmapInfo: CGImageAlphaInfo.premultipliedLast.rawValue)!
ctx.interpolationQuality = .high
let bw = CGFloat(bg.width), bh = CGFloat(bg.height), target = CGFloat(W) / CGFloat(H)
var cw = bw, ch = bh
if bw / bh > target { cw = bh * target } else { ch = bw / target }
let crop = bg.cropping(to: CGRect(x: (bw - cw) / 2, y: (bh - ch) / 2, width: cw, height: ch))!
ctx.draw(crop, in: CGRect(x: 0, y: 0, width: W, height: H))
// Figure like the select screen: centred around 66% of the width, feet cut off below the bottom edge.
let fh = CGFloat(H) * 1.18
let fw = fh * CGFloat(fig.width) / CGFloat(fig.height)
let fx = CGFloat(W) * 0.66 - fw / 2
let fy = CGFloat(H) * 0.97 - fh
ctx.draw(fig, in: CGRect(x: fx, y: fy, width: fw, height: fh))
let img = ctx.makeImage()!
let dest = CGImageDestinationCreateWithURL(URL(fileURLWithPath: out) as CFURL, UTType.png.identifier as CFString, 1, nil)!
CGImageDestinationAddImage(dest, img, nil)
CGImageDestinationFinalize(dest)
