using Blog.Models;
using Blog.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Blog.Features;

public class BlogService : IBlogService
{
    private readonly DataContext _dataContext;

    public BlogService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<PagedList<BlogPostResponse>> GetAllPosts(PageParameters pageParameters)
    {
        var lisPost = await (from bp in _dataContext.Set<BlogPost>()
            join ar in _dataContext.Set<Author>() on
                bp.AuthorId equals ar.AuthorId
            select new BlogPostResponse
            {
                AuthorsName = ar.FirstName + "" + ar.LastName,
                CoverImagePath = bp.CoverImagePath,
                Title = bp.Summary,
                Summary = bp.Summary,
                Body = bp.Body,
                Tags = bp.Tags,
                Category = bp.Category,
                DateCreated = bp.Created
            }).OrderByDescending(x => x.DateCreated).ToListAsync();

        return await PagedList<BlogPostResponse>.ToPagedList(lisPost, pageParameters.PageNumber,
            pageParameters.PageSize);
    }

    public async Task<BlogPost?> GetPostById(long id)
    {
        var post = await _dataContext.BlogPosts
            .Where(x => x.PostId == id).Include(x => x.Author)
            .Include(x => x.Category)
            .OrderBy(x => x.PostId)
            .FirstOrDefaultAsync();

        return post ?? null;
    }

    public async Task<PagedList<BlogPostResponse>> GetPostByTitle(string title, PageParameters pageParameters)
    {
        var post = await (from  bp in _dataContext.BlogPosts.Where(x =>x.Title.Contains(title))
            join  ar in _dataContext.Authors on 
                bp.AuthorId equals  ar.AuthorId
            select new BlogPostResponse
            {
                AuthorsName = ar.FirstName + "" + ar.LastName,
                CoverImagePath = bp.CoverImagePath,
                Title = bp.Summary,
                Summary = bp.Summary,
                Body = bp.Body,
                Tags = bp.Tags,
                Category = bp.Category,
                DateCreated = bp.Created
            }).OrderByDescending(x => x.DateCreated).ToListAsync();

        return await PagedList<BlogPostResponse>.ToPagedList(post, pageParameters.PageNumber, pageParameters.PageSize);
    }

    public async Task<PagedList<BlogPostResponse>> GetPostByAuthor(PageParameters pageParameters, long id)
    {
        if (await _dataContext.Authors
                .Where(x => x.AuthorId == id)
                .SingleOrDefaultAsync() == null) return null; 
       
        var post = await (from bp in _dataContext.BlogPosts
            join ar in _dataContext.Authors.Where(x=>x.AuthorId==id) on
                bp.AuthorId equals ar.AuthorId
            select new BlogPostResponse
            {
                AuthorsName = ar.FirstName + "" + ar.LastName,
                CoverImagePath = bp.CoverImagePath,
                Title = bp.Summary,
                Summary = bp.Summary,
                Body = bp.Body,
                Tags = bp.Tags,
                Category = bp.Category,
                DateCreated = bp.Created
            }).OrderByDescending(x => x.DateCreated).ToListAsync();

        return await PagedList<BlogPostResponse>.ToPagedList(post, pageParameters.PageNumber, pageParameters.PageSize);
    }

    public async Task<BlogPost?> AddPost(BlogPost newPost)
    {
        _dataContext.BlogPosts.Add(newPost);
        await _dataContext.SaveChangesAsync();

        return newPost;
    }

    public async Task<BlogPost?> UpdatePost(BlogPost updatePost)
    {
        _dataContext.BlogPosts.Update(updatePost);
        await _dataContext.SaveChangesAsync();
        return updatePost;
    }

    public async Task DeletePost(long id)
    {
        _dataContext.BlogPosts.Remove((await GetPostById(id))!);
        await _dataContext.SaveChangesAsync();
    }

    public async Task<Category?> GetCategoryByName(string? categoryName)
    {
        return await _dataContext.Categories
            .Where(x => x.CategoryName!.ToUpper() == categoryName!.ToUpper())
            .Include(x => x.BlogPosts).FirstOrDefaultAsync();
    }

    public async Task<Category?> AddCategory(Category category)
    {
        var validateCategory = await GetCategoryByName(category.CategoryName);
       
        if (validateCategory != null)
            return validateCategory;

        var newCategory = new Category
        {
            CategoryName = category.CategoryName
        };
        
        _dataContext.Categories.Add(newCategory);
        await _dataContext.SaveChangesAsync();
        
        return newCategory;
    }

    public Task<string> UploadFile(IFormFile file)
    {
        var uniqueFileName = Guid.NewGuid() + "_" + file.FileName;
        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "WebRoot/images/", uniqueFileName);

        file.CopyTo(new FileStream(imagePath, FileMode.Create));
        return Task.FromResult(uniqueFileName);
    }
}