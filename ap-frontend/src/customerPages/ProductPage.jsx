import { useParams, Link, useSearchParams } from 'react-router-dom';
import { useState, useEffect } from 'react';
import GetUserRole from '../components/GetUserRole.jsx';
import Reveal from '../components/Reveal.jsx';

const API_URL = import.meta.env.VITE_API_URL

function ProductPage() {
  const [products, setProducts] = useState([]);
  const [role, setRole] = useState(null);
  const [isLoading, setIsLoading] = useState(true);

  const { categoryName } = useParams();

  // Use the useSearchParams hook to get the sortBy query parameter from the URL. If it's not present, default to an empty string.
  const [searchParams, setSearchParams] = useSearchParams();
  const sortBy = searchParams.get("sortBy") ?? "";

  // Fetch the user role when the component mounts so that we can conditionally render admin features based on the user's role.
  useEffect(() => {
    async function fetchUserRole() {
      const userRole = await GetUserRole()
      setRole(userRole)
    }

    fetchUserRole()
  }, [])

  useEffect(() => {
    async function loadProducts() {
      setProducts([])

      try {

        const response = await fetch(`${API_URL}/api/products/category/${categoryName}?sortBy=${sortBy}`);

        if (!response.ok) {
          // If the response is not ok, wait for 5 seconds and try again
          await new Promise((resolve) => setTimeout(resolve, 5000));
          response = await fetch(`${API_URL}/api/products/category/${categoryName}?sortBy=${sortBy}`);
        }

        // Throw an error if the response is still not ok after retrying
        if (!response.ok) {
          throw new Error(`Fetching error! status: ${response.status}`);
        }

        const data = await response.json();
        setProducts(data);
        setIsLoading(false);
      }
      catch (error) {
        console.error(error);
      }
    }

    loadProducts();
    }, [categoryName, sortBy]);

  function handleSort(e) {
    const value = e.target.value

    setSearchParams({
      sortBy: value
    })
  }

  const categoryBackground = categoryName === 'new-arrival' ? 'bg-new-arrival'
    : categoryName === 'home-decor' ? 'bg-home-decor'
      : categoryName === 'gifts' ? 'bg-gifts'
        : categoryName === 'jewelry' ? 'bg-jewelry'
          : categoryName === 'art' ? 'bg-art' : '';

  const categoryTitle = categoryName === 'new-arrival' ? 'New Arrival'
    : categoryName === 'home-decor' ? 'Home Decor'
      : categoryName === 'gifts' ? 'Gifts'
        : categoryName === 'jewelry' ? 'Jewelry'
          : categoryName === 'art' ? 'Art' : '';

  const categoryDescription = categoryName === 'new-arrival' ? 'A quiet collection of new pieces chosen for warmth, detail, and the feeling they bring into a room.'
    : categoryName === 'home-decor' ? 'Warm objects and finishes chosen to bring stillness, texture, and character into everyday spaces.'
      : categoryName === 'gifts' ? 'Thoughtful pieces selected for personal moments, quiet celebrations, and meaningful keepsakes.'
        : categoryName === 'jewelry' ? 'Delicate accents and expressive details chosen for everyday wear and lasting sentiment.'
          : categoryName === 'art' ? 'Expressive pieces selected to shape the mood of a room and invite slower looking.' : '';

  const productLayout = categoryName === 'art'
    ? {
      gridColumns: 'md:grid-cols-2 lg:grid-cols-3',
      imageSize: 'aspect-[7/5]'
    }
    : {
      gridColumns: 'md:grid-cols-3 lg:grid-cols-4',
      imageSize: 'aspect-[5/6]'
    };

  return (
    <>
      <section className="relative min-h-screen">
        <div className={categoryBackground} />
        <div className="absolute inset-0 flex flex-col items-end justify-center pr-10 text-white md:pr-20 lg:pr-30">
          <Reveal key={`${categoryName}-title`}>
            <h1 className="font-['Mea_Culpa'] text-[3rem] font-thin tracking-[0.15em] transition duration-500 hover:translate-x-2 md:pr-30 md:text-[5rem] lg:pr-50 lg:text-[7rem]">
              {categoryTitle}
            </h1>
          </Reveal>
          <Reveal delay={0.4} key={`${categoryName}-description`}>
            <p className="mt-4 max-w-xs pr-0 text-right text-sm font-light leading-7 transition duration-500 hover:translate-x-2 md:max-w-xl md:pr-30 md:text-base md:leading-8 lg:max-w-2xl lg:pr-50 lg:text-lg lg:leading-9">
              {categoryDescription}
            </p>
          </Reveal>
        </div>
      </section>

      {/* Use a loading state to show a loading message while products are being fetched so that users don't see an empty page with no products */}
      {isLoading ? (
        <div className="flex justify-center items-center h-64">
          <p>Loading products...</p>
        </div>
      ) : (
        <main className="bg-ap-tan px-6 py-10 text-ap-brown md:px-12 lg:px-20">
          <div className="mx-auto mb-10 flex max-w-6xl items-center justify-between">
            {role == "Admin" ? (
              <div className="flex gap-3 md:gap-4 lg:gap-5">
                <Link to="/admin/add-product" className="cursor-pointer border border-ap-brown bg-ap-tan px-2 py-1 text-sm text-ap-brown transition duration-300 hover:bg-ap-pale md:px-5 md:py-2 md:text-base">
                  Add product
                </Link>
                <Link to="/admin/product-image" className="cursor-pointer border border-ap-brown bg-ap-tan px-2 py-1 text-sm text-ap-brown transition duration-300 hover:bg-ap-pale md:px-5 md:py-2 md:text-base">
                  Product images
                </Link>
              </div>
            ) : (
              <div></div>
            )}

            <select
              value={sortBy}
              onChange={(event) => handleSort(event)}
              className="cursor-pointer border border-ap-brown bg-ap-tan px-2 py-1 text-sm text-ap-brown transition duration-300 hover:bg-ap-pale focus:bg-ap-pale md:px-3 md:py-2 md:text-base"
            >
              <option value="">Sort by</option>
              <option value="price-low-to-high">Price: Low to High</option>
              <option value="price-high-to-low">Price: High to Low</option>
              <option value="name-a-to-z">Name: A to Z</option>
              <option value="name-z-to-a">Name: Z to A</option>
            </select>
          </div>

          {products.length > 0 ? (
            <div className={`mx-auto grid max-w-6xl grid-cols-2 gap-6 lg:gap-8 ${productLayout.gridColumns}`}>
              {products.map((product) => (
                <Reveal key={product.id} duration={1}>
                  <div className="group transition duration-300 hover:-translate-y-1">
                    <Link to={`/products/${categoryName}/${product.id}`} className="block">

                      <div className="overflow-hidden rounded">
                        {product.images?.[0]?.imageUrl ? (
                          <img
                            src={product.images[0].imageUrl}
                            alt={product.name}
                            className={`w-full object-cover object-center transition duration-500 group-hover:scale-105 ${productLayout.imageSize}`}
                          />
                        ) : (
                          <div className={`flex w-full items-center justify-center border border-ap-brown bg-ap-pale text-xs uppercase tracking-widest md:text-sm ${productLayout.imageSize}`}>
                            No image available
                          </div>
                        )}
                      </div>

                      {role == "Admin" ? (
                        <h2 className="mt-4 text-sm font-bold uppercase tracking-widest md:text-base lg:text-lg">Product Id: {product.id}</h2>
                      ) : null}

                      <h2 className="mt-4 font-['Tangerine'] text-3xl font-bold leading-none transition duration-200 group-hover:text-ap-beige md:text-4xl lg:text-5xl">
                        {product.name}
                      </h2>
                      <p className="mt-2 line-clamp-3 text-sm leading-6 md:text-base md:leading-7 lg:text-base">
                        {product.description}
                      </p>
                      <p className="mt-3 text-sm font-medium md:text-base lg:text-lg">
                        ${Number(product.price).toFixed(2)}
                      </p>
                    </Link>
                  </div>
                </Reveal>
              ))}
            </div>
          ) : (
            <p className="text-center">No products found.</p>
          )}
        </main >
      )}
    </>
  );
}

export default ProductPage;
